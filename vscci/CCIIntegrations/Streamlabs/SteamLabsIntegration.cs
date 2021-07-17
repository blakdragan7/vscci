
namespace vscci.CCIIntegrations.Streamlabs
{
    using System.Collections.Generic;
    using Vintagestory.API.Client;
    using Quobject.Collections.Immutable;
    using Quobject.EngineIoClientDotNet.Client.Transports;
    using Quobject.SocketIoClientDotNet.Client;
    using Newtonsoft.Json.Linq;
    using vscci.Data;

    public class SteamLabsIntegration
    {
        public const string SOCKET_TOKEN = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ0b2tlbiI6IkVGNTk3MkE2RTRGRDgyN0I0QTczIiwicmVhZF9vbmx5Ijp0cnVlLCJwcmV2ZW50X21hc3RlciI6dHJ1ZSwidHdpdGNoX2lkIjoiMTY3MjEzMjg3In0.5FhgvOEZERGaMzB5_wD_OU5-WkwJeRjyyGE388XGUj8";
        private ICoreClientAPI api;
        private Socket socket;
        private Dictionary<string,string> channels;

        public SteamLabsIntegration(ICoreClientAPI capi)
        {
            api = capi;
            socket = null;
            channels = new Dictionary<string, string>();
        }

        public void Connect()
        {
            if (socket == null)
            {
                CreateSocket();
            }
            socket.Connect();
        }

        public void Disconnect()
        {
            socket.Disconnect();
        }

        private void CreateSocket()
        {
            socket = IO.Socket($"wss://sockets.streamlabs.com",
                new IO.Options
                {
                    AutoConnect = false,
                    // Upgrade = true,
                    Transports = ImmutableList.Create(WebSocket.NAME),
                    QueryString = $"token={SOCKET_TOKEN}"
                });

            socket.On(Socket.EVENT_CONNECT, () =>
            {
                 api.Logger.Notification("Socket Connected");
            });

            socket.On(Socket.EVENT_DISCONNECT, (data) =>
            {
                 api.Logger.Notification("Socket Disconnected {0}", data);
            });

            socket.On(Socket.EVENT_ERROR, (data) =>
            {
                 api.Logger.Error("Socket Error: {0}", data);
            });

            socket.On("event", (data) =>
            {
                var str = data.ToString();
                var token = JToken.Parse(str);
                var type = token.SelectToken("type").ToString();
                switch (type)
                {
                    case "follow":
                        ParseFollow(token.SelectToken("message"));
                        break;
                    case "donation":
                        ParseDonation(token.SelectToken("message"));
                        break;
                    case "subscription":
                    case "resub":
                        ParseSubscription(token.SelectToken("message"));
                        break;
                    case "host":
                        ParseHost(token.SelectToken("message"));
                        break;
                    case "bits":
                        ParseBits(token.SelectToken("message"));
                        break;
                    case "raid":
                        ParseRaid(token.SelectToken("message"));
                        break;
                    case "superchat":
                        ParseSuperChat(token.SelectToken("message"));
                        break;
                    case "loyalty_store_redemption":
                        ParseLoyaltyStoreRedemption(token.SelectToken("message"));
                        break;
                    default:
                        break;
                }
            });
        }

        private void ParseFollow(JToken token)
        {
            foreach(var follow in token)
            {
                api.Event.PushEvent(Constants.EVENT_FOLLOW, new ProtoDataTypeAttribute<FollowData>(new FollowData()
                {
                    who = follow.SelectToken("name").ToString(),
                    channel = ""
                }));
            }
        }

        private void ParseDonation(JToken token)
        {
            foreach (var donation in token)
            {
                api.Event.PushEvent(Constants.EVENT_DONATION, new ProtoDataTypeAttribute<DonationData>(new DonationData()
                {
                    who = donation.SelectToken("name").ToString(),
                    amount = float.Parse(donation.SelectToken("amount").ToString()),
                    message = donation.SelectToken("message").ToString()
                }));
            }
        }

        private void ParseSubscription(JToken token)
        {
            foreach (var sub in token)
            {
                var monthToken = sub.SelectToken("months");
                api.Event.PushEvent(Constants.EVENT_NEW_SUB, new ProtoDataTypeAttribute<NewSubData>(new NewSubData()
                {
                    message = sub.SelectToken("message")?.ToString(),
                    to = sub.SelectToken("name").ToString(),
                    isGift = false,
                    months = monthToken != null ? (int)monthToken : 0
                }));
            }
        }

        private void ParseHost(JToken token)
        {
            foreach (var host in token)
            {
                api.Event.PushEvent(Constants.EVENT_HOST, new ProtoDataTypeAttribute<HostData>(new HostData()
                {
                    who = host.SelectToken("name").ToString(),
                    viewers = int.Parse(host.SelectToken("viewers").ToString())
                }));
            }
        }

        private void ParseBits(JToken token)
        {
            foreach (var bits in token)
            {
                api.Event.PushEvent(Constants.EVENT_BITS_RECIEVED, new ProtoDataTypeAttribute<BitsData>(new BitsData()
                {
                    from = bits.SelectToken("name").ToString(),
                    amount = int.Parse(bits.SelectToken("amount").ToString()),
                    message = bits.SelectToken("message").ToString()
                }));
            }
        }

        private void ParseRaid(JToken token)
        {
            foreach (var raid in token)
            {
                api.Event.PushEvent(Constants.EVENT_RAID, new ProtoDataTypeAttribute<RaidData>(new RaidData()
                {
                    raidChannel = raid.SelectToken("name").ToString(),
                    numberOfViewers = (int)raid.SelectToken("raiders")
                }));
            }
        }

        private void ParseSuperChat(JToken token)
        {
            foreach (var superChat in token)
            {
                var iamount = int.Parse(superChat.SelectToken("amount").ToString());
#pragma warning disable IDE0004 // Remove Unnecessary Cast
                float amount = (float)iamount / 100.0f;
#pragma warning restore IDE0004 // Remove Unnecessary Cast
                api.Event.PushEvent(Constants.EVENT_SCHAT, new ProtoDataTypeAttribute<SuperChatData>(new SuperChatData()
                {
                    who = superChat.SelectToken("name").ToString(),
                    amount = amount,
                    comment = superChat.SelectToken("comment").ToString()
                }));
            }
        }

        private void ParseLoyaltyStoreRedemption(JToken token)
        {
            foreach (var redemption in token)
            {
                api.Event.PushEvent(Constants.EVENT_REDEMPTION, new ProtoDataTypeAttribute<PointRedemptionData>(new PointRedemptionData()
                {
                    who = redemption.SelectToken("from").ToString(),
                    message = redemption.SelectToken("message").ToString(),
                    redemptionName = redemption.SelectToken("product").ToString(),
                }));
            }
        }

        /*private string PlatformFromChildToken(JToken token)
        {
            return token.Parent.SelectToken("for").ToString();
        }*/
    }
}
