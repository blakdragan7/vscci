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
        private TwitchPubSub client;
        //private Users userClient;
        private string authToken;
        private bool connected;
        private bool hasTopics;
        private bool isWaitingOnTopics;
        private IServerPlayer player;

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

            ta = new TwitchAutherizationHelper(sapi);
            ta.onAuthSucceful += onAuthSucceful;
            ta.onAuthFailed += onAuthError;
            ta.onAuthBecameInvalid += onAuthBecameInvalid;

            client = new TwitchPubSub();
            client.OnPubSubServiceConnected += onPubSubServiceConnected;
            client.OnPubSubServiceError += onPubServiceConnectionFailed;
            client.OnListenResponse += onListenResponse;
            client.OnBitsReceived += onBitsReceived;
            client.OnFollow += onFollows;
            client.OnRaidGo += onRaid;
            client.OnRewardRedeemed += onRewardRedeemed;
            client.OnChannelSubscription += onSubscription;
        }

        public void Reset()
        {
            IsTwitchPartnerAccount = false;
            IsFailedState = false;
            numberOfSuccesfulListens = 0;
            connected = false;
            hasTopics = false;
            isWaitingOnTopics = false;
            authToken = null;

            client.Disconnect();
            ta.EndValidationPing();
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
            client.Connect();
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
                api.BroadcastMessageToAllGroups($"{args.Username} gave {args.BitsUsed} with message {args.ChatMessage}", EnumChatType.Notification);
                api.Event.PushEvent(Constants.TWITCH_EVENT_BITS_RECIEVED, new ProtoDataTypeAttribute<TwitchBitsData>(new TwitchBitsData() {amount=args.BitsUsed, from=args.Username, message=args.ChatMessage }));
            }
        }

        private void onRewardRedeemed(object sender, OnRewardRedeemedArgs args)
        {
            if (args != null)
            {
                api.BroadcastMessageToAllGroups($"{args.DisplayName} redeemed {args.RewardTitle}", EnumChatType.Notification);
                api.Event.PushEvent(Constants.TWITCH_EVENT_REDEMPTION, new ProtoDataTypeAttribute<TwitchPointRedemptionData>(new TwitchPointRedemptionData() {redemptionID=args.RedemptionId.ToString(),redemptionName=args.RewardTitle,who=args.DisplayName, message=args.Message }));
            }
        }

        private void onFollows(object sender, OnFollowArgs args)
        {
            if (args != null)
            {
                api.BroadcastMessageToAllGroups($"{args.DisplayName} is now Following!", EnumChatType.Notification);
                api.Event.PushEvent(Constants.TWITCH_EVENT_FOLLOW, new ProtoDataTypeAttribute<TwitchFollowData>(new TwitchFollowData() {who=args.DisplayName}));
            }
        }

        private void onRaid(object sender, OnRaidGoArgs args)
        {
            if (args != null)
            {
                api.BroadcastMessageToAllGroups($"{args.ChannelId} is raiding with {args.ViewerCount} viewiers !", EnumChatType.Notification);
                api.Event.PushEvent(Constants.TWITCH_EVENT_RAID, new ProtoDataTypeAttribute<TwitchRaidData>(new TwitchRaidData() {raidChannel=args.Id.ToString(), numberOfViewers=args.ViewerCount}));
            }
        }

        private void onSubscription(object sender, OnChannelSubscriptionArgs args)
        {
            if (args != null)
            {
                if (args.Subscription.IsGift.GetValueOrDefault(false))
                {
                    api.BroadcastMessageToAllGroups($"{args.Subscription.DisplayName} Gifted Sub to {args.Subscription.RecipientDisplayName}!", EnumChatType.Notification);
                    api.Event.PushEvent(Constants.TWITCH_EVENT_NEW_SUB, new ProtoDataTypeAttribute<TwitchNewSubData>(new TwitchNewSubData() { isGift = true, from = args.Subscription.DisplayName, to=args.Subscription.RecipientDisplayName}));
                }
                else
                {
                    api.BroadcastMessageToAllGroups($"{args.Subscription.DisplayName} Subscribed with message {args.Subscription.SubMessage}", EnumChatType.Notification);
                    api.Event.PushEvent(Constants.TWITCH_EVENT_NEW_SUB, new ProtoDataTypeAttribute<TwitchNewSubData>(new TwitchNewSubData() {isGift=false, to=args.Subscription.DisplayName }));
                }
            }
        }

        private void onPubSubServiceConnected(object sender, EventArgs e)
        {
            connected = true;
            isWaitingOnTopics = true;

            api.World.Logger.Log(EnumLogType.Debug, $"Twitch CCI Connected");
            // these are only possible if the user is a twitch partner
            if (IsTwitchPartnerAccount)
            {
                client.ListenToSubscriptions(TwitchID);
                client.ListenToBitsEvents(TwitchID);
            }
            // these always work
            client.ListenToFollows(TwitchID);
            client.ListenToRaid(TwitchID);

            // SendTopics accepts an oauth optionally, which is necessary for some topics
            // If the user has not logged in yet then this will be null, which is allowed
            client.SendTopics(authToken);
        }

        private void onPubServiceConnectionFailed(object sender, OnPubSubServiceErrorArgs e)
        {
            OnConnectFailed?.Invoke(this, new OnConnectFailedArgs() { reason=e.Exception.Message, player = this.player });
        }

        private void onListenResponse(object sender, OnListenResponseArgs e)
        {
            if (e != null && !IsFailedState)
            {
                api.World.Logger.Chat($"onListenResponse:  was succeful {e.Successful} => wither error: {e.Response.Error}");

                if (e.Successful)
                {
                    numberOfSuccesfulListens++;
                    if ((IsTwitchPartnerAccount && numberOfSuccesfulListens == 4) || (!IsTwitchPartnerAccount && numberOfSuccesfulListens == 2))
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
                    client.Disconnect();
                }
            }
        }
    }
}
