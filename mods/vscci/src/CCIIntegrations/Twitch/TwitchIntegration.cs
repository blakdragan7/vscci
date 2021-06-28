//#define TWITCH_INTEGRATION_EVENT_TESTING

using System;

using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;

//using TwitchLib.Api.Core;
using TwitchLib.Api.Helix;
//using TwitchLib.Api.Core.RateLimiter;
//using TwitchLib.Api.Core.HttpCallHandlers;

using Vintagestory.API.Server;
using Vintagestory.API.Common;
using vscci.src.Data;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace vscci.src.CCIIntegrations.Twitch
{
    public class OnConnectFailedArgs : EventArgs
    {
        public string reason { get; set; }
        public IServerPlayer player { get; set; }

        public OnConnectFailedArgs() { }
    }

    public class OnAuthFailedArgs : EventArgs
    {
        public string Message { get; set; }
        public IServerPlayer player { get; set; }

        public OnAuthFailedArgs() { }
    }
    class TwitchIntegration
    {
        private string TwitchID = "MischiefOfMice";
        private int numberOfSuccesfulListens;
        //private Users userClient;
        private string authToken;
        private bool connected;
        private bool hasTopics;
        private bool isWaitingOnTopics;
        private IServerPlayer player;

#if TWITCH_INTEGRATION_EVENT_TESTING
        private TwitchTestServer twitchInterface;
#else
        private TwitchPubSub twitchInterface;
#endif

        private TwitchAutherizationHelper ta;

        private ICoreServerAPI api;

        public bool IsTwitchPartnerAccount { get; set; }
        public bool IsFailedState { get; set; }

        public event EventHandler<OnAuthFailedArgs> OnLoginError;
        public event EventHandler<IServerPlayer> OnLoginSuccess;

        public event EventHandler<IServerPlayer> OnConnectSuccess;
        public event EventHandler<OnConnectFailedArgs> OnConnectFailed;

        public TwitchIntegration(ICoreServerAPI sapi, IServerPlayer splayer)
        {
            api = sapi;
            player = splayer;
            IsTwitchPartnerAccount = false;
            IsFailedState = false;
            numberOfSuccesfulListens = 0;
            connected = false;
            hasTopics = false;
            isWaitingOnTopics = false;

            //ApiSettings settings = new ApiSettings();

            //settings.ClientId = Constants.TWITCH_CLIENT_ID;

            //TimeLimiter limiter = TimeLimiter.Compose();
            //TwitchHttpClient httpClient = new TwitchHttpClient();

            //userClient = new Users(settings, limiter, httpClient);
#if TWITCH_INTEGRATION_EVENT_TESTING
            twitchInterface = new TwitchTestServer();
#else
            twitchInterface = new TwitchPubSub();
#endif

            ta = new TwitchAutherizationHelper(sapi);
            ta.onAuthSucceful += onAuthSucceful;
            ta.onAuthFailed += onAuthError;
            ta.onAuthBecameInvalid += onAuthBecameInvalid;

            twitchInterface.OnPubSubServiceConnected += onPubSubServiceConnected;
            twitchInterface.OnPubSubServiceError += onPubServiceConnectionFailed;
            twitchInterface.OnListenResponse += onListenResponse;
            twitchInterface.OnBitsReceived += onBitsReceived;
            twitchInterface.OnFollow += onFollows;
            twitchInterface.OnRaidGo += onRaid;
            twitchInterface.OnRewardRedeemed += onRewardRedeemed;
            twitchInterface.OnChannelSubscription += onSubscription;
        }

        public void Reset()
        {
            twitchInterface.Disconnect();
            ta.EndValidationPing();
            IsTwitchPartnerAccount = false;
            IsFailedState = false;
            numberOfSuccesfulListens = 0;
            connected = false;
            hasTopics = false;
            isWaitingOnTopics = false;
        }

        public void Connect(bool isPartner = false)
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
            IsTwitchPartnerAccount = isPartner;
            twitchInterface.Connect();

        }

        public string StartSignInFlow()
        {
            return ta.StartAuthFlow();
        }

        public string GetAuthDataForSaving()
        {
            return authToken;
        }

        public void SetAuthDataFromSaveData(string savedAuth)
        {
            if (ta.ValidateToken(savedAuth))
            {
                authToken = savedAuth;
                ta.BeginValidationPingForToken(authToken, Constants.AUTH_VALIDATION_INTERVAL);
            }
            else
            {
                api.SendMessage(player, player.Groups[0].GroupUid, "Auth Token Became Invalid, please re-connect with twitch", EnumChatType.Notification);
            }
        }

        private void onAuthSucceful(object sender, string token)
        {
            authToken = token;
            OnLoginSuccess?.Invoke(this, player);

            ta.BeginValidationPingForToken(authToken, Constants.AUTH_VALIDATION_INTERVAL);
        }

        private void onAuthError(object sender, string errorMessage)
        {
            OnLoginError?.Invoke(this, new OnAuthFailedArgs() {Message= errorMessage,player=this.player });
        }

        private void onAuthBecameInvalid(object sender,string nowInvalidAuth)
        {
            if(nowInvalidAuth == authToken)
            {
                authToken = null;
                // delete cache of auth token
                api.SendMessage(player, player.Groups[0].GroupUid, "Auth Token Became Invalid, please re-connect with twitch", EnumChatType.Notification);
            }    
        }

        private void onBitsReceived(object sender, OnBitsReceivedArgs args)
        {
            if (args != null)
            {
                var data = new TwitchBitsData() { amount = args.BitsUsed, from = args.Username, message = args.ChatMessage };
                api.BroadcastMessageToAllGroups($"{args.Username} gave {args.BitsUsed} with message {args.ChatMessage}", EnumChatType.Notification);
                api.Event.PushEvent(Constants.TWITCH_EVENT_BITS_RECIEVED, new ProtoDataTypeAttribute<TwitchBitsData>(data));
                api.Network.GetChannel(Constants.NETWORK_EVENT_CHANNEL).SendPacket(data, new[] { player});
            }
        }

        private void onRewardRedeemed(object sender, OnRewardRedeemedArgs args)
        {
            if (args != null)
            {
                var data = new TwitchPointRedemptionData() { redemptionID = args.RedemptionId.ToString(), redemptionName = args.RewardTitle, who = args.DisplayName, message = args.Message };

                api.BroadcastMessageToAllGroups($"{args.DisplayName} redeemed {args.RewardTitle}", EnumChatType.Notification);
                api.Event.PushEvent(Constants.TWITCH_EVENT_REDEMPTION, new ProtoDataTypeAttribute<TwitchPointRedemptionData>(data));
                api.Network.GetChannel(Constants.NETWORK_EVENT_CHANNEL).SendPacket(data, new[] { player});
            }
        }

        private void onFollows(object sender, OnFollowArgs args)
        {
            if (args != null)
            {
                var data = new TwitchFollowData() { who = args.DisplayName };
                api.BroadcastMessageToAllGroups($"{args.DisplayName} is now Following {args.FollowedChannelId}!", EnumChatType.Notification);
                api.Event.PushEvent(Constants.TWITCH_EVENT_FOLLOW, new ProtoDataTypeAttribute<TwitchFollowData>(data));
                api.Network.GetChannel(Constants.NETWORK_EVENT_CHANNEL).SendPacket(data, new[] { player});
            }
        }

        private void onRaid(object sender, OnRaidGoArgs args)
        {
            if (args != null)
            {
                var data = new TwitchRaidData() { raidChannel = args.Id.ToString(), numberOfViewers = args.ViewerCount };

                api.BroadcastMessageToAllGroups($"{args.ChannelId} is raiding with {args.ViewerCount} viewiers !", EnumChatType.Notification);
                api.Event.PushEvent(Constants.TWITCH_EVENT_RAID, new ProtoDataTypeAttribute<TwitchRaidData>(data));
                api.Network.GetChannel(Constants.NETWORK_EVENT_CHANNEL).SendPacket(data, new[] { player});
            }
        }

        private void onSubscription(object sender, OnChannelSubscriptionArgs args)
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

        private void onPubSubServiceConnected(object sender, EventArgs e)
        {
            connected = true;
            isWaitingOnTopics = true;

            api.World.Logger.Log(EnumLogType.Debug, "Twitch CCI Connected");
            // these are only possible if the user is a twitch partner
            if (IsTwitchPartnerAccount)
            {
                twitchInterface.ListenToSubscriptions(TwitchID);
                twitchInterface.ListenToBitsEvents(TwitchID);
            }
            // these always work
            twitchInterface.ListenToFollows(TwitchID);
            twitchInterface.ListenToRaid(TwitchID);
            twitchInterface.ListenToRewards(TwitchID);

            // SendTopics accepts an oauth optionally, which is necessary for some topics
            // If the user has not logged in yet then this will be null, which is allowed
            twitchInterface.SendTopics(authToken);
        }

        private void onPubServiceConnectionFailed(object sender, OnPubSubServiceErrorArgs e)
        {
            OnConnectFailed?.Invoke(this, new OnConnectFailedArgs() { reason=e.Exception.Message, player = this.player });
        }

        private void onListenResponse(object sender, OnListenResponseArgs e)
        {
            if (e != null && !IsFailedState)
            {
                api.World.Logger.Chat("onListenResponse:  was succeful {0} => wither error: {1}", e.Successful, e.Response.Error);

                if (e.Successful)
                {
                    numberOfSuccesfulListens++;
                    if ((IsTwitchPartnerAccount && numberOfSuccesfulListens == 5) || (!IsTwitchPartnerAccount && numberOfSuccesfulListens == 3))
                    {
                        isWaitingOnTopics = false;
                        hasTopics = true;
                        OnConnectSuccess?.Invoke(this, player);
                    }
                }
                else
                {
                    isWaitingOnTopics = false;
                    hasTopics = false;
                    connected = false;
                    IsFailedState = true;
                    numberOfSuccesfulListens = 0;
                    OnConnectFailed?.Invoke(this, new OnConnectFailedArgs() { reason=$"Listen Failed for tpoic: {e.Topic} with response: {e.Response}", player = this.player });
                    twitchInterface.Disconnect();
                }
            }
        }
    }
}
