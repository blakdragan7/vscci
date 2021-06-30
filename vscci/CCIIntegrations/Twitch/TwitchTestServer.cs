namespace vscci.CCIIntegrations.Twitch
{
    using System;
    using System.Collections.Generic;

    using System.Net;
    using vscci.Data;
    using TwitchLib.PubSub.Events;
    using TwitchLib.PubSub.Models.Responses.Messages;
    using Vintagestory.API.Common;
    using TwitchLib.PubSub.Models.Responses;

    class TwitchTestServer
    {
        private HttpListener listener;
        private string testSubJson;
        private string testGiftSubJson;

        private bool RaidListen;
        private bool BitListen;
        private bool SubListen;
        private bool FollowListen;
        private bool RewardsListen;

        private bool ShouoldSimulateError;

        public event EventHandler OnPubSubServiceClosed;
        public event EventHandler OnPubSubServiceConnected;
        public event EventHandler<OnPubSubServiceErrorArgs> OnPubSubServiceError;
        public event EventHandler<OnListenResponseArgs> OnListenResponse;
        public event EventHandler<OnBitsReceivedArgs> OnBitsReceived;
        public event EventHandler<OnChannelSubscriptionArgs> OnChannelSubscription;
        public event EventHandler<OnFollowArgs> OnFollow;
        public event EventHandler<OnRewardRedeemedArgs> OnRewardRedeemed;
        public event EventHandler<OnRaidGoArgs> OnRaidGo;

        public TwitchTestServer()
        {
            listener = new HttpListener();
            listener.Prefixes.Add(Constants.LISTEN_PREFIX);

            RaidListen     = false;
            BitListen      = false;
            SubListen      = false;
            FollowListen   = false;
            RewardsListen  = false;

            ShouoldSimulateError = false;

            /*
                public SubscriptionPlan SubscriptionPlan { get; }
             */

            Dictionary<string, object> dsm = new Dictionary<string, object>
            {
                { "message", "Channel Sub Test" },
                { "emotes", null }
            };

            Dictionary<string, object> ds = new Dictionary<string, object>
            {
                { "context", "test context" },
                { "streak_months", 1 },
                { "cumulative_months", 1 },
                { "months", 1 },
                { "time", DateTime.Now.ToString() },
                { "multi_month_duration", 1 },
                { "sub_plan_name", "test plan" },
                { "sub_plan", "prime" },
                { "channel_id", "testchannel" },
                { "channel_name", "testchannel" },
                { "display_name", "testuser" },
                { "recipient_display_name", "" },
                { "recipient_id", 0 },
                { "recipient_user_name", "" },
                { "sub_message", dsm },
                { "user_id", "0123456789" },
                { "user_name", "testuser" },
                { "is_gift", false }
            };

            testSubJson = JsonUtil.ToString(ds);

            Console.WriteLine(testSubJson);

            dsm["message"] = "Channel Gift Sub Test";

            ds["is_gift"] = true;
            ds["recipient_user_name"] = "testgifteduser";
            ds["recipient_id"] = "1234567890";
            ds["recipient_display_name"] = "testgifteduser";
            ds["sub_message"] = dsm;

            testGiftSubJson = JsonUtil.ToString(ds);
        }

        public void Connect()
        {
            if(ShouoldSimulateError)
            {
                OnPubSubServiceError?.Invoke(this, new OnPubSubServiceErrorArgs() { Exception=new Exception("testSubJson Pub Error Exception") });
                return;
            }

            listener.Start();
            listener.BeginGetContext(OnHttpCallback, this);

            OnPubSubServiceConnected?.Invoke(this, null);
        }

        public void Disconnect()
        {
            listener.Stop();
        }

        public void ListenToSubscriptions(string ChannelID)
        {
            SubListen = true;
        }

        public void ListenToBitsEvents(string ChannelID)
        {
            BitListen = true;
        }

        public void ListenToFollows(string ChannelID)
        {
            FollowListen = true;
        }

        public void ListenToRaid(string ChannelID)
        {
            RaidListen = true;
        }

        public void ListenToRewards(string ChannelID)
        {
            RewardsListen = true;
        }

        public void SendTopics(string OAuthToken)
        {
            if(SubListen)
            {
                OnListenResponse?.Invoke(this, new OnListenResponseArgs() { Response = new Response("{\"Error\":\"null\",\"Nonce\":\"\",\"Succesful\":true}"), Successful = true, Topic = "twitch:sub" });
            }

            if (RaidListen)
            {
                OnListenResponse?.Invoke(this, new OnListenResponseArgs() { Response = new Response("{\"Error\":\"null\",\"Nonce\":\"\",\"Succesful\":true}"), Successful = true, Topic = "twitch:raid" });
            }

            if (FollowListen)
            {
                OnListenResponse?.Invoke(this, new OnListenResponseArgs() { Response = new Response("{\"Error\":\"null\",\"Nonce\":\"\",\"Succesful\":true}"), Successful = true, Topic = "twitch:follow" });
            }

            if (BitListen)
            {
                OnListenResponse?.Invoke(this, new OnListenResponseArgs() { Response = new Response("{\"Error\":\"null\",\"Nonce\":\"\",\"Succesful\":true}"), Successful = true, Topic = "twitch:bit" });
            }

            if (RewardsListen)
            {
                OnListenResponse?.Invoke(this, new OnListenResponseArgs() { Response = new Response("{\"Error\":\"null\",\"Nonce\":\"\",\"Succesful\":true}"), Successful = true, Topic = "twitch:reward" });
            }
        }

        private void OnHttpCallback(IAsyncResult result)
        {
            var context = listener.EndGetContext(result);
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            if (request.Url.AbsolutePath == "/bit")
            {
                OnBitsReceived?.Invoke(this, new OnBitsReceivedArgs() { BitsUsed = 10, ChannelId = "testchannel", ChannelName="testchannel", ChatMessage="This is a test bit event", Context="random context string", UserId="123456789", Username="testuser", TotalBitsUsed=10, Time="000000000" });
            }

            else if (request.Url.AbsolutePath == "/sub")
            {
                var sub = new ChannelSubscription(testSubJson);
                OnChannelSubscription?.Invoke(this, new OnChannelSubscriptionArgs() { ChannelId = "testchannel", Subscription = sub });
            }

            else if (request.Url.AbsolutePath == "/subg")
            {
                var sub = new ChannelSubscription(testGiftSubJson);
                OnChannelSubscription?.Invoke(this, new OnChannelSubscriptionArgs() { ChannelId = "testchannel", Subscription = sub});
            }

            else if (request.Url.AbsolutePath == "/follow")
            {
                OnFollow?.Invoke(this, new OnFollowArgs() { DisplayName="testuser", FollowedChannelId="testchannel", UserId="0123456789", Username="testuser" });
            }

            else if (request.Url.AbsolutePath == "/raid")
            {
                OnRaidGo?.Invoke(this, new OnRaidGoArgs() { ChannelId="testchannelraider", Id=Guid.NewGuid(), TargetChannelId="testchannel", TargetDisplayName="testchannel", TargetLogin="testchannel", TargetProfileImage="None", ViewerCount=1 });
            }

            else if (request.Url.AbsolutePath == "/redemption")
            {
                OnRewardRedeemed?.Invoke(this, new OnRewardRedeemedArgs() { ChannelId="testchannel", DisplayName="testuser", Login="testuser", Message="I test reward redemption", RedemptionId=Guid.NewGuid(), RewardCost=0, RewardId=Guid.NewGuid(), RewardPrompt="test prompt", RewardTitle="test reward", Status="Some status", TimeStamp=DateTime.Now });
            }

            else
            {
                response.StatusCode = 404;
            }

            response.Close();
            listener.BeginGetContext(OnHttpCallback, this);
        }
    }
}
