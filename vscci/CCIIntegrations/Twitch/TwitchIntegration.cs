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
    using System.Collections.Generic;

    internal class TwitchIntegration : CCIInterface
    {
        private string twitchUsername;
        private string twitchID;
        private int numberOfSuccesfulListens;
        private bool connected;
        private bool hasTopics;
        private bool isWaitingOnTopics;

#if TWITCH_INTEGRATION_EVENT_TESTING
        private readonly TwitchTestServer pubSubClient;
#else
        private readonly TwitchPubSub pubSubClient;
#endif
        private readonly TwitchAPI apiClient;
        private readonly TwitchAutherizationHelper ta;

        private readonly ICoreClientAPI api;
        private string authToken;

        public bool IsTwitchCommerceAccount { get; set; }
        public bool IsFailedState { get; set; }


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
            pubSubClient.OnHost += OnHost;
        }

        public override void Reset()
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

        public override void Connect()
        {
            if (connected && hasTopics)
            {
                CallConnectSuccess();
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

        public override void Disconnect()
        {
            if (connected == false)
            {
                return;
            }

            pubSubClient.Disconnect();

            isWaitingOnTopics = false;
            hasTopics = false;
            connected = false;
            IsFailedState = false;
            numberOfSuccesfulListens = 0;
        }

        public void StartSignInFlow()
        {
            if (authToken != null)
            {
                // we are already connected
                CallLoginSuccess(authToken);
                return;
            }
            ta.StartAuthFlow();
        }

        public override string GetAuthDataForSaving()
        {
            return authToken;
        }

        public override async void SetRawAuthData(string authData)
        {
            if (authToken != authData)
            {
                if (ta.ValidateToken(authData))
                {
                    Reset();

                    authToken = authData;
                    ta.BeginValidationPingForToken(authToken, Constants.AUTH_VALIDATION_INTERVAL);

                    apiClient.Settings.AccessToken = authToken;

                    var result = await apiClient.Helix.Users.GetUsersAsync(null, null, authToken);
                    if (result.Users.Length > 0)
                    {
                        twitchID = result.Users[0].Id;
                        twitchUsername = result.Users[0].DisplayName;
                        IsTwitchCommerceAccount = result.Users[0].BroadcasterType == "partner" || result.Users[0].BroadcasterType == "affiliate";
                        api.Event.PushEvent(Constants.CCI_EVENT_LOGIN_UPDATE, new ProtoDataTypeAttribute<CCILoginUpdate>(new CCILoginUpdate() { id = twitchID, user = twitchUsername }));
                        CallLoginSuccess(null);
                    }
                }
            }
        }

        public override async void SetAuthDataFromSaveData(string savedAuth)
        {
            if (ta.ValidateToken(savedAuth))
            {
                Reset();

                authToken = savedAuth;
                ta.BeginValidationPingForToken(authToken, Constants.AUTH_VALIDATION_INTERVAL);

                apiClient.Settings.AccessToken = authToken;

                var result = await apiClient.Helix.Users.GetUsersAsync(null, null, authToken);
                if (result.Users.Length > 0)
                {
                    twitchID = result.Users[0].Id;
                    twitchUsername = result.Users[0].DisplayName;
                    IsTwitchCommerceAccount = result.Users[0].BroadcasterType == "partner" || result.Users[0].BroadcasterType == "affiliate";
                    api.Event.PushEvent(Constants.CCI_EVENT_LOGIN_UPDATE, new ProtoDataTypeAttribute<CCILoginUpdate>(new CCILoginUpdate() { id = twitchID, user = twitchUsername }));
                    CallLoginSuccess(null);
                }
            }
            else
            {
                api.ShowChatMessage("Auth Token Became Invalid, please re-connect with twitch");
            }
        }

        private async void OnAuthSucceful(object sender, string token)
        {
            authToken = token;
            apiClient.Settings.AccessToken = authToken;

            CallLoginSuccess(token);

            ta.BeginValidationPingForToken(authToken, Constants.AUTH_VALIDATION_INTERVAL);

            var result = await apiClient.Helix.Users.GetUsersAsync(null, null, authToken);
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
            CallLoginError(new OnAuthFailedArgs() { Message = errorMessage });
        }

        private void OnAuthBecameInvalid(object sender, string nowInvalidAuth)
        {
            if (nowInvalidAuth == authToken)
            {
                authToken = null;
                twitchID = "None";
                twitchUsername = "None";

                api.ShowChatMessage("Auth Token Became Invalid, please re-connect with twitch");
                api.Event.PushEvent(Constants.CCI_EVENT_LOGIN_UPDATE, new ProtoDataTypeAttribute<CCILoginUpdate>(new CCILoginUpdate() { id = twitchID, user = twitchUsername }));
            }
        }

        private async void OnHost(object sender, OnHostArgs e)
        {
            if(e != null)
            {
                api.Event.PushEvent(Constants.EVENT_HOST, new ProtoDataTypeAttribute<HostData>(new HostData()
                {
                    who = await GetChannelNameForId(e.ChannelId),
                    viewers = await GetChannelViewersForId(e.ChannelId)
                }));
            }
        }

        private void OnBitsReceived(object sender, OnBitsReceivedArgs args)
        {
            if (args != null)
            {
                var data = new BitsData() { amount = args.BitsUsed, from = args.Username, message = args.ChatMessage };
                //api.BroadcastMessageToAllGroups($"{args.Username} gave {args.BitsUsed} with message {args.ChatMessage}", EnumChatType.Notification);
                api.Event.PushEvent(Constants.EVENT_BITS_RECIEVED, new ProtoDataTypeAttribute<BitsData>(data));
            }
        }

        private void OnRewardRedeemed(object sender, OnRewardRedeemedArgs args)
        {
            if (args != null)
            {
                var data = new PointRedemptionData() { redemptionID = args.RedemptionId.ToString(), redemptionName = args.RewardTitle, who = args.DisplayName, message = args.Message };

                //api.BroadcastMessageToAllGroups($"{args.DisplayName} redeemed {args.RewardTitle}", EnumChatType.Notification);
                api.Event.PushEvent(Constants.EVENT_REDEMPTION, new ProtoDataTypeAttribute<PointRedemptionData>(data));
            }
        }

        private async void OnFollows(object sender, OnFollowArgs args)
        {
            if (args != null)
            {
                var channelName = await GetChannelNameForId(args.FollowedChannelId);
                var data = new FollowData() { who = args.DisplayName, channel = channelName, platform="Twitch" };
                //api.BroadcastMessageToAllGroups($"{args.DisplayName} is now Following {channelName}!", EnumChatType.Notification);
                api.Event.PushEvent(Constants.EVENT_FOLLOW, new ProtoDataTypeAttribute<FollowData>(data));
            }
        }

        private async void OnRaid(object sender, OnRaidGoArgs args)
        {
            if (args != null)
            {
                var channelName = await GetChannelNameForId(args.ChannelId);

                var data = new RaidData() { raidChannel = channelName, numberOfViewers = args.ViewerCount };

                //api.BroadcastMessageToAllGroups($"{channelName} is raiding with {args.ViewerCount} viewiers !", EnumChatType.Notification);
                api.Event.PushEvent(Constants.EVENT_RAID, new ProtoDataTypeAttribute<RaidData>(data));
            }
        }

        private void OnSubscription(object sender, OnChannelSubscriptionArgs args)
        {
            if (args != null)
            {
                if (args.Subscription.IsGift.GetValueOrDefault(false))
                {
                    var data = new NewSubData() { isGift = true, from = args.Subscription.DisplayName, to = args.Subscription.RecipientDisplayName };

                    //api.BroadcastMessageToAllGroups($"{args.Subscription.DisplayName} Gifted Sub to {args.Subscription.RecipientDisplayName}!", EnumChatType.Notification);
                    api.Event.PushEvent(Constants.EVENT_NEW_SUB, new ProtoDataTypeAttribute<NewSubData>(data));
                }
                else
                {
                    var data = new NewSubData() { isGift = false, to = args.Subscription.DisplayName, message=args.Subscription.SubMessage.Message };

                    //api.BroadcastMessageToAllGroups($"{args.Subscription.DisplayName} Subscribed with message {args.Subscription.SubMessage.Message}", EnumChatType.Notification);
                    api.Event.PushEvent(Constants.EVENT_NEW_SUB, new ProtoDataTypeAttribute<NewSubData>(data));
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
            api.Event.PushEvent(Constants.CCI_EVENT_CONNECT_UPDATE, new ProtoDataTypeAttribute<CCIConnectionUpdate>(new CCIConnectionUpdate() { type = CCIType.Twitch, status = "Disconnected" }));
        }

        private void OnPubServiceConnectionFailed(object sender, OnPubSubServiceErrorArgs e)
        {
            if (e.Exception.GetType() == typeof(OperationCanceledException))
            {
                // if the reqeust was just cancceled then act like disconnect
                api.Event.PushEvent(Constants.CCI_EVENT_CONNECT_UPDATE, new ProtoDataTypeAttribute<CCIConnectionUpdate>(new CCIConnectionUpdate() { type = CCIType.Twitch, status = "Disconnected" }));
            }
            else
            {
                api.Event.PushEvent(Constants.CCI_EVENT_CONNECT_UPDATE, new ProtoDataTypeAttribute<CCIConnectionUpdate>(new CCIConnectionUpdate() { type = CCIType.Twitch, status = $"Failed With Error {e.Exception.Message}" }));
                CallConnectFailed(new OnConnectFailedArgs() { Reason = e.Exception.Message });
            }
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
                        CallConnectSuccess();

                        api.Event.PushEvent(Constants.CCI_EVENT_CONNECT_UPDATE, new ProtoDataTypeAttribute<CCIConnectionUpdate>(new CCIConnectionUpdate() { type = CCIType.Twitch, status = "Connected" }));
                    }
                }
                else
                {
                    isWaitingOnTopics = false;
                    hasTopics = false;
                    connected = false;
                    IsFailedState = true;
                    numberOfSuccesfulListens = 0;
                    CallConnectFailed(new OnConnectFailedArgs() { Reason = $"Listen Failed for tpoic: {e.Topic} with response: {e.Response}" });
                    pubSubClient.Disconnect();
                    api.Event.PushEvent(Constants.CCI_EVENT_CONNECT_UPDATE, new ProtoDataTypeAttribute<CCIConnectionUpdate>(new CCIConnectionUpdate() { type = CCIType.Twitch, status = $"Listen Failed for tpoic: {e.Topic} with response: {e.Response}" }));
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


        private async Task<int> GetChannelViewersForId(string channelID)
        {
            try
            {
                var userIds = new List<string>()
                {
                    channelID
                };

                var channelInfo = await apiClient.Helix.Streams.GetStreamsAsync(null, null, 1, null, null, "live", userIds, null);

                if (channelInfo.Streams.Length > 0)
                {
                    return channelInfo.Streams[0].ViewerCount;
                }

                return 0;
            }
            catch
            {
                api.Logger.Warning("Could Not find channel with id {0}", channelID);
            }

            return 0;
        }
    }
}
