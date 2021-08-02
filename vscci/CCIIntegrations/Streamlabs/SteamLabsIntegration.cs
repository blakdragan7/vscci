
namespace VSCCI.CCIIntegrations.Streamlabs
{
    using Vintagestory.API.Client;
    using Quobject.Collections.Immutable;
    using Quobject.EngineIoClientDotNet.Client.Transports;
    using Quobject.SocketIoClientDotNet.Client;
    using Newtonsoft.Json.Linq;
    using VSCCI.Data;
    using VSCCI.CCINetworkTypes;

    public class SteamLabsIntegration : CCIIntegrationBase
    {
        private readonly ICoreClientAPI api;
        private Socket socket;
        private string authToken;
        private bool connected;

        public SteamLabsIntegration(ICoreClientAPI capi)
        {
            api = capi;
            socket = null;
            connected = false;
        }

        public override void Connect()
        {
            if (socket == null)
            {
                CreateSocket();
            }

            socket.Connect();
        }

        public override void Disconnect()
        {
            socket?.Disconnect();
        }

        public override void Reset()
        {
            if (connected)
            {
                socket?.Disconnect();
            }
            connected = false;
            socket = null;
        }

        public override string GetAuthDataForSaving()
        {
            return authToken;
        }

        public override void SetRawAuthData(string authData)
        {
            if (authToken != authData)
            {
                authToken = authData;

                // reset socket if we changed auth token
                if (connected)
                {
                    socket.Disconnect();
                    socket = null;
                }
            }
        }

        public override void SetAuthDataFromSaveData(string savedAuth)
        {
            authToken = savedAuth;

            // create / re-create the socket to update the token info
            // connect automaitcally from save

            CreateSocket();
            Connect();
        }

        private void CreateSocket()
        {
            if (socket != null)
            {
                if (connected)
                {
                    socket.Disconnect();
                }
                socket = null;
            }

            socket = IO.Socket($"wss://sockets.streamlabs.com",
                new IO.Options
                {
                    AutoConnect = false,
                    // Upgrade = true,
                    Transports = ImmutableList.Create(WebSocket.NAME),
                    QueryString = $"token={authToken}"
                });

            socket.On(Socket.EVENT_CONNECT, () =>
            {
                api.Logger.Notification("Streamlabs Socket Connected");
                api.Event.EnqueueMainThreadTask(() => api.ShowChatMessage("Streamlabs Socket Connected"), null);
                connected = true;

                CallLoginSuccess(authToken);
                CallConnectSuccess();
                api.Event.PushEvent(Constants.CCI_EVENT_CONNECT_UPDATE, new ProtoDataTypeAttribute<CCIConnectionUpdate>(new CCIConnectionUpdate() { type = CCIType.Streamlabs, status = "Connected" }));
            });

            socket.On(Socket.EVENT_DISCONNECT, (data) =>
            {
                api.Logger.Notification("Streamlabs Socket Disconnected {0}", data);
                api.Event.EnqueueMainThreadTask(() => api.ShowChatMessage($"Streamlabs Socket Disconnected {data}"), null);
                api.Event.PushEvent(Constants.CCI_EVENT_CONNECT_UPDATE, new ProtoDataTypeAttribute<CCIConnectionUpdate>(new CCIConnectionUpdate() { type = CCIType.Streamlabs, status = "Disconnected" }));

                connected = false;
            });

            socket.On(Socket.EVENT_ERROR, (data) =>
            {
                api.Logger.Error("Streamlabs Socket Error: {0}", data);
                api.Event.EnqueueMainThreadTask(() => api.ShowChatMessage($"Streamlabs Socket Error: {data}"), null);
                connected = false;

                api.Event.PushEvent(Constants.CCI_EVENT_CONNECT_UPDATE, new ProtoDataTypeAttribute<CCIConnectionUpdate>(new CCIConnectionUpdate() { type = CCIType.Streamlabs, status = "Error" }));
                CallLoginError(new OnAuthFailedArgs() { Message = data.ToString() });
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
