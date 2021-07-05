//#define TWITCH_INTEGRATION_EVENT_TESTING
namespace vscci.CCIIntegrations.Twitch
{
    using System;
#if !TWITCH_INTEGRATION_EVENT_TESTING
    using TwitchLib.PubSub;
#endif
    using TwitchLib.PubSub.Events;

    using TwitchLib.Api;

    using Vintagestory.API.Client;
    using Vintagestory.API.Common;

    using System.Threading.Tasks;
    using vscci.CCINetworkTypes;
    using vscci.Data;

    public class OnConnectFailedArgs : EventArgs
    {
        public string Reason { get; set; }

        public OnConnectFailedArgs() { }
    }

    public class OnAuthFailedArgs : EventArgs
    {
        public string Message { get; set; }

        public OnAuthFailedArgs() { }
    }
    public class TwitchIntegration
    {
        private string twitchUsername;
        private string twitchID;
        private int numberOfSuccesfulListens;
        private bool connected;
        private bool hasTopics;
        private bool isWaitingOnTopics;

#if TWITCH_INTEGRATION_EVENT_TESTING
        private TwitchTestServer pubSubClient;
#else
        private readonly TwitchPubSub pubSubClient;
#endif
        private readonly TwitchAPI apiClient;
        private readonly TwitchAutherizationHelper ta;

        private readonly ICoreClientAPI api;

        public bool IsTwitchCommerceAccount { get; set; }
        public bool IsFailedState { get; set; }

        public string AuthToken { get; set; }

        public event EventHandler<OnAuthFailedArgs> OnLoginError;
        public event EventHandler<string> OnLoginSuccess;

        public event EventHandler OnConnectSuccess;
        public event EventHandler<OnConnectFailedArgs> OnConnectFailed;

        public TwitchIntegration(ICoreClientAPI capi)
        {
            api = capi;
            IsTwitchCommerceAccount = false;
            IsFailedState = false;
            numberOfSuccesfulListens = 0;
            connected = false;
            hasTopics = false;
            isWaitingOnTopics = false;
            twitchID = null;

#if TWITCH_INTEGRATION_EVENT_TESTING
            pubSubClient = new TwitchTestServer();
#else
            pubSubClient = new TwitchPubSub();
#endif

            ta = new TwitchAutherizationHelper(capi);
            ta.OnAuthSucceful += OnAuthSucceful;
            ta.OnAuthFailed += OnAuthError;
            ta.OnAuthBecameInvalid += OnAuthBecameInvalid;

            apiClient = new TwitchAPI();
            apiClient.Settings.ClientId = Constants.TWITCH_CLIENT_ID;

            pubSubClient.OnPubSubServiceClosed += OnPubSubServiceDisconnected;
            pubSubClient.OnPubSubServiceConnected += OnPubSubServiceConnected;
            pubSubClient.OnPubSubServiceError += OnPubServiceConnectionFailed;
            pubSubClient.OnListenResponse += OnListenResponse;
            pubSubClient.OnBitsReceived += OnBitsReceived;
            pubSubClient.OnFollow += OnFollows;
            pubSubClient.OnRaidGo += OnRaid;
            pubSubClient.OnRewardRedeemed += OnRewardRedeemed;
            pubSubClient.OnChannelSubscription += OnSubscription;
        }

        public void Reset()
        {
            pubSubClient.Disconnect();
            ta.EndValidationPing();
            IsTwitchCommerceAccount = false;
            IsFailedState = false;
            numberOfSuccesfulListens = 0;
            connected = false;
            hasTopics = false;
            isWaitingOnTopics = false;
        }

        public void Connect()
        {
            if (connected && hasTopics)
            {
                OnConnectSuccess?.Invoke(this, null);
                return;
            }
            else if (isWaitingOnTopics)
            {
                return;
            }

            IsFailedState = false;
            numberOfSuccesfulListens = 0;
            pubSubClient.Connect();

        }

        public void StartSignInFlow()
        {
            if (AuthToken != null)
            {
                // we are already connected
                OnLoginSuccess?.Invoke(this, AuthToken);
                return;
            }
            ta.StartAuthFlow();
        }

        public string GetAuthDataForSaving()
        {
            return AuthToken;
        }

        public async void SetAuthDataFromSaveData(string savedAuth)
        {
            if (ta.ValidateToken(savedAuth))
            {
                AuthToken = savedAuth;
                ta.BeginValidationPingForToken(AuthToken, Constants.AUTH_VALIDATION_INTERVAL);

                apiClient.Settings.AccessToken = AuthToken;

                var result = await apiClient.Helix.Users.GetUsersAsync(null, null, AuthToken);
                if (result.Users.Length > 0)
                {
                    twitchID = result.Users[0].Id;
                    twitchUsername = result.Users[0].DisplayName;
                    IsTwitchCommerceAccount = result.Users[0].BroadcasterType == "partner" || result.Users[0].BroadcasterType == "affiliate";
                    api.Event.PushEvent(Constants.CCI_EVENT_LOGIN_UPDATE, new ProtoDataTypeAttribute<CCILoginUpdate>(new CCILoginUpdate() { id = twitchID, user = twitchUsername }));
                }
            }
            else
            {
                api.ShowChatMessage("Auth Token Became Invalid, please re-connect with twitch");
            }
        }

        private async void OnAuthSucceful(object sender, string token)
        {
            AuthToken = token;
            apiClient.Settings.AccessToken = AuthToken;

            OnLoginSuccess?.Invoke(this, token);

            ta.BeginValidationPingForToken(AuthToken, Constants.AUTH_VALIDATION_INTERVAL);

            var result = await apiClient.Helix.Users.GetUsersAsync(null, null, AuthToken);
            if (result.Users.Length > 0)
            {
                twitchID = result.Users[0].Id;
                twitchUsername = result.Users[0].DisplayName;
                IsTwitchCommerceAccount = result.Users[0].BroadcasterType == "partner" || result.Users[0].BroadcasterType == "affiliate";
                api.Event.PushEvent(Constants.CCI_EVENT_LOGIN_UPDATE, new ProtoDataTypeAttribute<CCILoginUpdate>(new CCILoginUpdate() { id = twitchID, user = twitchUsername }));
            }
        }

        private void OnAuthError(object sender, string errorMessage)
        {
            OnLoginError?.Invoke(this, new OnAuthFailedArgs() { Message = errorMessage });
        }

        private void OnAuthBecameInvalid(object sender, string nowInvalidAuth)
        {
            if (nowInvalidAuth == AuthToken)
            {
                AuthToken = null;
                twitchID = "None";
                twitchUsername = "None";

                api.ShowChatMessage("Auth Token Became Invalid, please re-connect with twitch");
                api.Event.PushEvent(Constants.CCI_EVENT_LOGIN_UPDATE, new ProtoDataTypeAttribute<CCILoginUpdate>(new CCILoginUpdate() { id = twitchID, user = twitchUsername }));
            }
        }

        private void OnBitsReceived(object sender, OnBitsReceivedArgs args)
        {
            if (args != null)
            {
                var data = new TwitchBitsData() { amount = args.BitsUsed, from = args.Username, message = args.ChatMessage };
                //api.BroadcastMessageToAllGroups($"{args.Username} gave {args.BitsUsed} with message {args.ChatMessage}", EnumChatType.Notification);
                api.Event.PushEvent(Constants.TWITCH_EVENT_BITS_RECIEVED, new ProtoDataTypeAttribute<TwitchBitsData>(data));
            }
        }

        private void OnRewardRedeemed(object sender, OnRewardRedeemedArgs args)
        {
            if (args != null)
            {
                var data = new TwitchPointRedemptionData() { redemptionID = args.RedemptionId.ToString(), redemptionName = args.RewardTitle, who = args.DisplayName, message = args.Message };

                //api.BroadcastMessageToAllGroups($"{args.DisplayName} redeemed {args.RewardTitle}", EnumChatType.Notification);
                api.Event.PushEvent(Constants.TWITCH_EVENT_REDEMPTION, new ProtoDataTypeAttribute<TwitchPointRedemptionData>(data));
            }
        }

        private async void OnFollows(object sender, OnFollowArgs args)
        {
            if (args != null)
            {
                var channelName = await GetChannelNameForId(args.FollowedChannelId);
                var data = new TwitchFollowData() { who = args.DisplayName, channel = channelName };
                //api.BroadcastMessageToAllGroups($"{args.DisplayName} is now Following {channelName}!", EnumChatType.Notification);
                api.Event.PushEvent(Constants.TWITCH_EVENT_FOLLOW, new ProtoDataTypeAttribute<TwitchFollowData>(data));
            }
        }

        private async void OnRaid(object sender, OnRaidGoArgs args)
        {
            if (args != null)
            {
                var channelName = await GetChannelNameForId(args.ChannelId);

                var data = new TwitchRaidData() { raidChannel = channelName, numberOfViewers = args.ViewerCount };

                //api.BroadcastMessageToAllGroups($"{channelName} is raiding with {args.ViewerCount} viewiers !", EnumChatType.Notification);
                api.Event.PushEvent(Constants.TWITCH_EVENT_RAID, new ProtoDataTypeAttribute<TwitchRaidData>(data));
            }
        }

        private void OnSubscription(object sender, OnChannelSubscriptionArgs args)
        {
            if (args != null)
            {
                if (args.Subscription.IsGift.GetValueOrDefault(false))
                {
                    var data = new TwitchNewSubData() { isGift = true, from = args.Subscription.DisplayName, to = args.Subscription.RecipientDisplayName };

                    //api.BroadcastMessageToAllGroups($"{args.Subscription.DisplayName} Gifted Sub to {args.Subscription.RecipientDisplayName}!", EnumChatType.Notification);
                    api.Event.PushEvent(Constants.TWITCH_EVENT_NEW_SUB, new ProtoDataTypeAttribute<TwitchNewSubData>(data));
                }
                else
                {
                    var data = new TwitchNewSubData() { isGift = false, to = args.Subscription.DisplayName, message=args.Subscription.SubMessage.Message };

                    //api.BroadcastMessageToAllGroups($"{args.Subscription.DisplayName} Subscribed with message {args.Subscription.SubMessage.Message}", EnumChatType.Notification);
                    api.Event.PushEvent(Constants.TWITCH_EVENT_NEW_SUB, new ProtoDataTypeAttribute<TwitchNewSubData>(data));
                }
            }
        }

        private void OnPubSubServiceConnected(object sender, EventArgs e)
        {
            connected = true;
            isWaitingOnTopics = true;

            api.World.Logger.Log(EnumLogType.Debug, "Twitch CCI Connected");
            // these are only possible if the user is a twitch partner
            if (IsTwitchCommerceAccount)
            {
                pubSubClient.ListenToSubscriptions(twitchID);
                pubSubClient.ListenToBitsEvents(twitchID);
            }
            // these always work
            pubSubClient.ListenToFollows(twitchID);
            pubSubClient.ListenToRaid(twitchID);
            pubSubClient.ListenToRewards(twitchID);

            // SendTopics accepts an oauth optionally, which is necessary for some topics
            // If the user has not logged in yet then this will be null, which is allowed
            pubSubClient.SendTopics(AuthToken);
        }

        private void OnPubSubServiceDisconnected(object sender, EventArgs e)
        {
            api.Event.PushEvent(Constants.CCI_EVENT_CONNECT_UPDATE, new ProtoDataTypeAttribute<CCIConnectionUpdate>(new CCIConnectionUpdate() { status = "Disconnected" }));
        }

        private void OnPubServiceConnectionFailed(object sender, OnPubSubServiceErrorArgs e)
        {
            api.Event.PushEvent(Constants.CCI_EVENT_CONNECT_UPDATE, new ProtoDataTypeAttribute<CCIConnectionUpdate>(new CCIConnectionUpdate() { status = $"Failed With Error ${e.Exception.Message}" }));
            OnConnectFailed?.Invoke(this, new OnConnectFailedArgs() { Reason = e.Exception.Message });
        }

        private void OnListenResponse(object sender, OnListenResponseArgs e)
        {
            if (e != null && !IsFailedState)
            {
                api.World.Logger.Chat("onListenResponse:  was succeful {0} => wither error: {1}", e.Successful, e.Response.Error);

                if (e.Successful)
                {
                    numberOfSuccesfulListens++;
                    if ((IsTwitchCommerceAccount && numberOfSuccesfulListens == 5) || (!IsTwitchCommerceAccount && numberOfSuccesfulListens == 3))
                    {
                        isWaitingOnTopics = false;
                        hasTopics = true;
                        OnConnectSuccess?.Invoke(this, null);

                        api.Event.PushEvent(Constants.CCI_EVENT_CONNECT_UPDATE, new ProtoDataTypeAttribute<CCIConnectionUpdate>(new CCIConnectionUpdate() { status = "Connected" }));
                    }
                }
                else
                {
                    isWaitingOnTopics = false;
                    hasTopics = false;
                    connected = false;
                    IsFailedState = true;
                    numberOfSuccesfulListens = 0;
                    OnConnectFailed?.Invoke(this, new OnConnectFailedArgs() { Reason = $"Listen Failed for tpoic: {e.Topic} with response: {e.Response}" });
                    pubSubClient.Disconnect();
                    api.Event.PushEvent(Constants.CCI_EVENT_CONNECT_UPDATE, new ProtoDataTypeAttribute<CCIConnectionUpdate>(new CCIConnectionUpdate() { status = $"Listen Failed for tpoic: {e.Topic} with response: {e.Response}" }));
                }
            }
        }

        // if failes, just returns the original id
        private async Task<string> GetChannelNameForId(string channelID)
        {
            try
            {
                var channelInfo = await apiClient.Helix.Channels.GetChannelInformationAsync(channelID);

                if (channelInfo.Data.Length > 0)
                {
                    return channelInfo.Data[0].BroadcasterName;
                }

                return channelID;
            }
            catch
            {
                api.Logger.Warning("Could Not find channel with id {0}", channelID);
            }

            return channelID;
        }
    }
}
