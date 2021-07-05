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

    public class TwitchTestServer
    {
        private readonly HttpListener listener;
        private readonly string testSubJson;
        private readonly string testGiftSubJson;

        private bool raidListen;
        private bool bitListen;
        private bool subListen;
        private bool followListen;
        private bool rewardsListen;

        private readonly bool shouoldSimulateError;

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

            raidListen = false;
            bitListen = false;
            subListen = false;
            followListen = false;
            rewardsListen = false;

            shouoldSimulateError = false;

            /*
                public SubscriptionPlan SubscriptionPlan { get; }
             */

            var dsm = new Dictionary<string, object>
            {
                { "message", "Channel Sub Test" },
                { "emotes", null }
            };

            var ds = new Dictionary<string, object>
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
            if (shouoldSimulateError)
            {
                OnPubSubServiceError?.Invoke(this, new OnPubSubServiceErrorArgs() { Exception = new Exception("testSubJson Pub Error Exception") });
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
            subListen = true;
        }

        public void ListenToBitsEvents(string ChannelID)
        {
            bitListen = true;
        }

        public void ListenToFollows(string ChannelID)
        {
            followListen = true;
        }

        public void ListenToRaid(string ChannelID)
        {
            raidListen = true;
        }

        public void ListenToRewards(string ChannelID)
        {
            rewardsListen = true;
        }

        public void SendTopics(string OAuthToken)
        {
            if (subListen)
            {
                OnListenResponse?.Invoke(this, new OnListenResponseArgs() { Response = new Response("{\"Error\":\"null\",\"Nonce\":\"\",\"Succesful\":true}"), Successful = true, Topic = "twitch:sub" });
            }

            if (raidListen)
            {
                OnListenResponse?.Invoke(this, new OnListenResponseArgs() { Response = new Response("{\"Error\":\"null\",\"Nonce\":\"\",\"Succesful\":true}"), Successful = true, Topic = "twitch:raid" });
            }

            if (followListen)
            {
                OnListenResponse?.Invoke(this, new OnListenResponseArgs() { Response = new Response("{\"Error\":\"null\",\"Nonce\":\"\",\"Succesful\":true}"), Successful = true, Topic = "twitch:follow" });
            }

            if (bitListen)
            {
                OnListenResponse?.Invoke(this, new OnListenResponseArgs() { Response = new Response("{\"Error\":\"null\",\"Nonce\":\"\",\"Succesful\":true}"), Successful = true, Topic = "twitch:bit" });
            }

            if (rewardsListen)
            {
                OnListenResponse?.Invoke(this, new OnListenResponseArgs() { Response = new Response("{\"Error\":\"null\",\"Nonce\":\"\",\"Succesful\":true}"), Successful = true, Topic = "twitch:reward" });
            }
        }

        private void OnHttpCallback(IAsyncResult result)
        {
            if (result.IsCompleted)
            {
                try
                {
                    var context = listener.EndGetContext(result);
                    var request = context.Request;
                    var response = context.Response;

                    if (request.Url.AbsolutePath == "/bit")
                    {
                        OnBitsReceived?.Invoke(this, new OnBitsReceivedArgs() { BitsUsed = 10, ChannelId = "testchannel", ChannelName = "testchannel", ChatMessage = "This is a test bit event", Context = "random context string", UserId = "123456789", Username = "testuser", TotalBitsUsed = 10, Time = "000000000" });
                    }

                    else if (request.Url.AbsolutePath == "/sub")
                    {
                        var sub = new ChannelSubscription(testSubJson);
                        OnChannelSubscription?.Invoke(this, new OnChannelSubscriptionArgs() { ChannelId = "testchannel", Subscription = sub });
                    }

                    else if (request.Url.AbsolutePath == "/subg")
                    {
                        var sub = new ChannelSubscription(testGiftSubJson);
                        OnChannelSubscription?.Invoke(this, new OnChannelSubscriptionArgs() { ChannelId = "testchannel", Subscription = sub });
                    }

                    else if (request.Url.AbsolutePath == "/follow")
                    {
                        OnFollow?.Invoke(this, new OnFollowArgs() { DisplayName = "testuser", FollowedChannelId = "testchannel", UserId = "0123456789", Username = "testuser" });
                    }

                    else if (request.Url.AbsolutePath == "/raid")
                    {
                        OnRaidGo?.Invoke(this, new OnRaidGoArgs() { ChannelId = "testchannelraider", Id = Guid.NewGuid(), TargetChannelId = "testchannel", TargetDisplayName = "testchannel", TargetLogin = "testchannel", TargetProfileImage = "None", ViewerCount = 1 });
                    }

                    else if (request.Url.AbsolutePath == "/redemption")
                    {
                        OnRewardRedeemed?.Invoke(this, new OnRewardRedeemedArgs() { ChannelId = "testchannel", DisplayName = "testuser", Login = "testuser", Message = "I test reward redemption", RedemptionId = Guid.NewGuid(), RewardCost = 0, RewardId = Guid.NewGuid(), RewardPrompt = "test prompt", RewardTitle = "test reward", Status = "Some status", TimeStamp = DateTime.Now });
                    }

                    else
                    {
                        response.StatusCode = 404;
                    }

                    response.Close();
                    listener.BeginGetContext(OnHttpCallback, this);
                }
                catch(Exception exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
        }
    }
}
