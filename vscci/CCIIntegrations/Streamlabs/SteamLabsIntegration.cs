
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
                var token = JToken.Parse(data.ToString());
                var type = token.SelectToken("type").ToString();
                switch (type)
                {
                    case "follow":
                        ParseFollow(token.SelectToken("message"));
                        break;
                    case "donation":
                        ParseDonation(token.SelectToken("message"));
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
                api.Event.PushEvent(Constants.EVENT_FOLLOW, new ProtoDataTypeAttribute<DonationData>(new DonationData()
                {
                    who = donation.SelectToken("name").ToString(),
                    amount = donation.SelectToken("amount").ToString(),
                    message = donation.SelectToken("message").ToString()
                }));
            }
        }

        /*private string PlatformFromChildToken(JToken token)
        {
            return token.Parent.SelectToken("for").ToString();
        }*/
    }
}
