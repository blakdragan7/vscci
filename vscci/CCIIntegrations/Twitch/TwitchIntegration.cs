//#define TWITCH_INTEGRATION_EVENT_TESTING
namespace vscci.CCIIntegrations.Twitch
{
    using System;

    using TwitchLib.PubSub;
    using TwitchLib.PubSub.Events;

    using TwitchLib.Api;

    using Vintagestory.API.Server;
    using Vintagestory.API.Common;

    using System.Threading.Tasks;
    using vscci.CCINetworkTypes;
    using vscci.Data;

    public class OnConnectFailedArgs : EventArgs
    {
        public string Reason { get; set; }
        public IServerPlayer Player { get; set; }

        public OnConnectFailedArgs() { }
    }

    public class OnAuthFailedArgs : EventArgs
    {
        public string Message { get; set; }
        public IServerPlayer Player { get; set; }

        public OnAuthFailedArgs() { }
    }
    class TwitchIntegration
    {
        //private string                      TwitchID = "MischiefOfMice";
        //private string                      TwitchID = "ChosenArchitect";
        private string                      twitchUsername;
        private string                      twitchID;
        private int                         numberOfSuccesfulListens;
        private string                      authToken;
        private bool                        connected;
        private bool                        hasTopics;
        private bool                        isWaitingOnTopics;
        private IServerPlayer               player;

#if TWITCH_INTEGRATION_EVENT_TESTING
        private TwitchTestServer pubSubClient;
#else
        private TwitchPubSub                pubSubClient;
#endif
        private TwitchAPI                   apiClient;
        private TwitchAutherizationHelper   ta;

        private ICoreServerAPI              api;

        public bool IsTwitchCommerceAccount { get; set; }
        public bool IsFailedState { get; set; }

        public event EventHandler<OnAuthFailedArgs> OnLoginError;
        public event EventHandler<IServerPlayer> OnLoginSuccess;

        public event EventHandler<IServerPlayer> OnConnectSuccess;
        public event EventHandler<OnConnectFailedArgs> OnConnectFailed;

        public TwitchIntegration(ICoreServerAPI sapi, IServerPlayer splayer)
        {
            api = sapi;
            player = splayer;
            IsTwitchCommerceAccount = false;
            IsFailedState = false;
            numberOfSuccesfulListens = 0;
            connected = false;
            hasTopics = false;
            isWaitingOnTopics = false;
            twitchID = null;

            //ApiSettings settings = new ApiSettings();

            //settings.ClientId = Constants.TWITCH_CLIENT_ID;

            //TimeLimiter limiter = TimeLimiter.Compose();
            //TwitchHttpClient httpClient = new TwitchHttpClient();

            //userClient = new Users(settings, limiter, httpClient);
#if TWITCH_INTEGRATION_EVENT_TESTING
            pubSubClient = new TwitchTestServer();
#else
            pubSubClient = new TwitchPubSub();
#endif

            ta = new TwitchAutherizationHelper(sapi);
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
            if(connected && hasTopics)
            {
                OnConnectSuccess?.Invoke(this, null);
                return;
            }
            else if(isWaitingOnTopics)
            {
                return;
            }

            IsFailedState = false;
            numberOfSuccesfulListens = 0;
            pubSubClient.Connect();

        }

        public void StartSignInFlow()
        {
            ta.StartAuthFlow(player);
        }

        public string GetAuthDataForSaving()
        {
            return authToken;
        }

        public async void SetAuthDataFromSaveData(string savedAuth)
        {
            if (ta.ValidateToken(savedAuth))
            {
                authToken = savedAuth;
                ta.BeginValidationPingForToken(authToken, Constants.AUTH_VALIDATION_INTERVAL);

                apiClient.Settings.AccessToken = authToken;

                var result = await apiClient.Helix.Users.GetUsersAsync(null, null, authToken);
                if (result.Users.Length > 0)
                {
                    twitchID = result.Users[0].Id;
                    twitchUsername = result.Users[0].DisplayName;
                    IsTwitchCommerceAccount = (result.Users[0].BroadcasterType == "partner" || result.Users[0].BroadcasterType == "affiliate");
                    api.Network.GetChannel(Constants.NETWORK_GUI_CHANNEL).SendPacket(new CCILoginUpdate() { id = twitchID, user = twitchUsername }, player);
                }
            }
            else
            {
                api.SendMessage(player, player.Groups[0].GroupUid, "Auth Token Became Invalid, please re-connect with twitch", EnumChatType.Notification);
            }
        }

        private async void OnAuthSucceful(object sender, string token)
        {
            authToken = token;
            apiClient.Settings.AccessToken = authToken;

            OnLoginSuccess?.Invoke(this, player);

            ta.BeginValidationPingForToken(authToken, Constants.AUTH_VALIDATION_INTERVAL);

            var result = await apiClient.Helix.Users.GetUsersAsync(null, null, authToken);
            if (result.Users.Length > 0)
            {
                twitchID = result.Users[0].Id;
                twitchUsername = result.Users[0].DisplayName;
                IsTwitchCommerceAccount = (result.Users[0].BroadcasterType == "partner" || result.Users[0].BroadcasterType == "affiliate");
                api.Network.GetChannel(Constants.NETWORK_GUI_CHANNEL).SendPacket(new CCILoginUpdate() { id = twitchID, user = twitchUsername }, player);
            }
        }

        private void OnAuthError(object sender, string errorMessage)
        {
            OnLoginError?.Invoke(this, new OnAuthFailedArgs() {Message= errorMessage,Player=this.player });
        }

        private void OnAuthBecameInvalid(object sender,string nowInvalidAuth)
        {
            if(nowInvalidAuth == authToken)
            {
                authToken = null;
                // delete cache of auth token
                api.SendMessage(player, player.Groups[0].GroupUid, "Auth Token Became Invalid, please re-connect with twitch", EnumChatType.Notification);
            }    
        }

        private void OnBitsReceived(object sender, OnBitsReceivedArgs args)
        {
            if (args != null)
            {
                var data = new TwitchBitsData() { amount = args.BitsUsed, from = args.Username, message = args.ChatMessage };
                api.BroadcastMessageToAllGroups($"{args.Username} gave {args.BitsUsed} with message {args.ChatMessage}", EnumChatType.Notification);
                api.Event.PushEvent(Constants.TWITCH_EVENT_BITS_RECIEVED, new ProtoDataTypeAttribute<TwitchBitsData>(data));
                api.Network.GetChannel(Constants.NETWORK_EVENT_CHANNEL).SendPacket(data, new[] { player});
            }
        }

        private void OnRewardRedeemed(object sender, OnRewardRedeemedArgs args)
        {
            if (args != null)
            {
                var data = new TwitchPointRedemptionData() { redemptionID = args.RedemptionId.ToString(), redemptionName = args.RewardTitle, who = args.DisplayName, message = args.Message };

                api.BroadcastMessageToAllGroups($"{args.DisplayName} redeemed {args.RewardTitle}", EnumChatType.Notification);
                api.Event.PushEvent(Constants.TWITCH_EVENT_REDEMPTION, new ProtoDataTypeAttribute<TwitchPointRedemptionData>(data));
                api.Network.GetChannel(Constants.NETWORK_EVENT_CHANNEL).SendPacket(data, new[] { player});
            }
        }

        private async void OnFollows(object sender, OnFollowArgs args)
        {
            if (args != null)
            {
                var channelName = await GetChannelNameForId(args.FollowedChannelId);
                var data = new TwitchFollowData() { who = args.DisplayName };
                api.BroadcastMessageToAllGroups($"{args.DisplayName} is now Following {channelName}!", EnumChatType.Notification);
                api.Event.PushEvent(Constants.TWITCH_EVENT_FOLLOW, new ProtoDataTypeAttribute<TwitchFollowData>(data));
                api.Network.GetChannel(Constants.NETWORK_EVENT_CHANNEL).SendPacket(data, new[] { player});
            }
        }

        private async void OnRaid(object sender, OnRaidGoArgs args)
        {
            if (args != null)
            {
                var channelName = await GetChannelNameForId(args.ChannelId);

                var data = new TwitchRaidData() { raidChannel = channelName, numberOfViewers = args.ViewerCount };

                api.BroadcastMessageToAllGroups($"{channelName} is raiding with {args.ViewerCount} viewiers !", EnumChatType.Notification);
                api.Event.PushEvent(Constants.TWITCH_EVENT_RAID, new ProtoDataTypeAttribute<TwitchRaidData>(data));
                api.Network.GetChannel(Constants.NETWORK_EVENT_CHANNEL).SendPacket(data, new[] { player});
            }
        }

        private void OnSubscription(object sender, OnChannelSubscriptionArgs args)
        {
            if (args != null)
            {
                if (args.Subscription.IsGift.GetValueOrDefault(false))
                {
                    var data = new TwitchNewSubData() { isGift = true, from = args.Subscription.DisplayName, to = args.Subscription.RecipientDisplayName };

                    api.BroadcastMessageToAllGroups($"{args.Subscription.DisplayName} Gifted Sub to {args.Subscription.RecipientDisplayName}!", EnumChatType.Notification);
                    api.Event.PushEvent(Constants.TWITCH_EVENT_NEW_SUB, new ProtoDataTypeAttribute<TwitchNewSubData>(data));
                    api.Network.GetChannel(Constants.NETWORK_EVENT_CHANNEL).SendPacket(data, new[] { player});
                }
                else
                {
                    var data = new TwitchNewSubData() { isGift = false, to = args.Subscription.DisplayName };

                    api.BroadcastMessageToAllGroups($"{args.Subscription.DisplayName} Subscribed with message {args.Subscription.SubMessage.Message}", EnumChatType.Notification);
                    api.Event.PushEvent(Constants.TWITCH_EVENT_NEW_SUB, new ProtoDataTypeAttribute<TwitchNewSubData>(data));
                    api.Network.GetChannel(Constants.NETWORK_EVENT_CHANNEL).SendPacket(data, new[] { player});
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
            pubSubClient.SendTopics(authToken);
        }

        private void OnPubSubServiceDisconnected(object sender, EventArgs e)
        {
            api.Network.GetChannel(Constants.NETWORK_GUI_CHANNEL).SendPacket(new CCIConnectionUpdate() { status = "Disconnected" }, player);
        }

        private void OnPubServiceConnectionFailed(object sender, OnPubSubServiceErrorArgs e)
        {
            OnConnectFailed?.Invoke(this, new OnConnectFailedArgs() { Reason=e.Exception.Message, Player = this.player });
            api.Network.GetChannel(Constants.NETWORK_GUI_CHANNEL).SendPacket(new CCIConnectionUpdate() { status=$"Failed With Error ${e.Exception.Message}" }, player);
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
                        OnConnectSuccess?.Invoke(this, player);

                        api.Network.GetChannel(Constants.NETWORK_GUI_CHANNEL).SendPacket(new CCIConnectionUpdate() { status="Connected" }, player);
                    }
                }
                else
                {
                    isWaitingOnTopics = false;
                    hasTopics = false;
                    connected = false;
                    IsFailedState = true;
                    numberOfSuccesfulListens = 0;
                    OnConnectFailed?.Invoke(this, new OnConnectFailedArgs() { Reason=$"Listen Failed for tpoic: {e.Topic} with response: {e.Response}", Player = this.player });
                    pubSubClient.Disconnect();
                    api.Network.GetChannel(Constants.NETWORK_GUI_CHANNEL).SendPacket(new CCIConnectionUpdate() { status="Failed Registering To Events" }, player);
                }
            }
        }

        // if failes, just returns the original id
        private async Task<string> GetChannelNameForId(string channelID)
        {
            var channelInfo = await apiClient.Helix.Channels.GetChannelInformationAsync(channelID);

            if(channelInfo.Data.Length > 0)
            {
                return channelInfo.Data[0].BroadcasterName;
            }

            return channelID;
        }
    }   
}
