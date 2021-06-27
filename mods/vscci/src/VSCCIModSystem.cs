using System.Collections.Generic;
using vscci.src.CCIIntegrations.Twitch;
using vscci.src.CCINetworkTypes;

using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using vscci.src.Data;

namespace vscci.src
{
    class VSCCIModSystem : ModSystem
    {
        // server side variables
        private Dictionary<IServerPlayer, TwitchIntegration> dti;
        private ICoreServerAPI sapi;

        // client side variables
        private ICoreClientAPI capi;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.Network.RegisterChannel(Constants.NETWORK_CHANNEL)
                .RegisterMessageType(typeof(CCILoginStep))
                .RegisterMessageType(typeof(CCILoginRequest))
                .RegisterMessageType(typeof(CCIConnectRequest))
                .RegisterMessageType(typeof(CCIRequestResponse));
        }
        #region Server
        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            dti = new Dictionary<IServerPlayer, TwitchIntegration>();

            api.Network.GetChannel(Constants.NETWORK_CHANNEL)
                .SetMessageHandler<CCILoginRequest>(OnCCILoginRequest)
                .SetMessageHandler<CCIConnectRequest>(OnCCIConnectRequest)
            ;
            sapi = api;

            api.Event.SaveGameLoaded += OnGameLoad;
            api.Event.GameWorldSave += OnGameSave;
        }

        // this would normally be private but it's public so SaveDataUtil can access it
        public TwitchIntegration TIForPlayer(IServerPlayer player)
        {
            TwitchIntegration ti;
            if(dti.TryGetValue(player, out ti))
            {
                return ti;
            }
            else
            {
                ti = new TwitchIntegration(sapi, player);

                ti.OnConnectFailed += OnCCIConnectFailed;
                ti.OnConnectSuccess += OnCCIConnect;
                ti.OnLoginError += OnCCILoginFailed;
                ti.OnLoginSuccess += OnCCILogin;

                dti.Add(player, ti);

                return ti;
            }
        }

        private void OnGameLoad()
        {
            SaveDataUtil.LoadAuthData(sapi, this);
        }

        private void OnGameSave()
        {
            SaveDataUtil.SaveAuthData(sapi, dti);
        }

        private void OnCCILoginRequest(IPlayer fromPlayer, CCILoginRequest request)
        {
            var url = TIForPlayer(fromPlayer as IServerPlayer).StartSignInFlow();
            sapi.Network.GetChannel(Constants.NETWORK_CHANNEL).SendPacket<CCILoginStep>(new CCILoginStep() {url= url }, new[] {fromPlayer as IServerPlayer });
        }

        private void OnCCIConnectRequest(IPlayer fromPlayer, CCIConnectRequest request)
        {
            TIForPlayer(fromPlayer as IServerPlayer).Connect(request.istwitchpartner);
        }

        private void OnCCILogin(object sender, IServerPlayer player)
        {
            sapi.Network.GetChannel(Constants.NETWORK_CHANNEL).SendPacket<CCIRequestResponse>(new CCIRequestResponse() { requestType = "login", response = $"success", success = true }, new[] { player });
        }

        private void OnCCILoginFailed(object sender, OnAuthFailedArgs args)
        {
            sapi.Network.GetChannel(Constants.NETWORK_CHANNEL).SendPacket<CCIRequestResponse>(new CCIRequestResponse() { requestType = "login", response = args.Message, success = false }, new[] { args.player });
        }

        private void OnCCIConnect(object sender, IServerPlayer player)
        {
            sapi.Network.GetChannel(Constants.NETWORK_CHANNEL).SendPacket<CCIRequestResponse>(new CCIRequestResponse() { requestType = "connect", response = $"success", success = true }, new[] { player });
        }

        private void OnCCIConnectFailed(object sender, OnConnectFailedArgs args)
        {
            sapi.Network.GetChannel(Constants.NETWORK_CHANNEL).SendPacket<CCIRequestResponse>(new CCIRequestResponse() { requestType = "connect", response = args.reason, success = false }, new[] { args.player });
        }
        #endregion

        #region Client
        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);

            api.Network.GetChannel(Constants.NETWORK_CHANNEL)
                .SetMessageHandler<CCIRequestResponse>(OnRequestResponse)
                .SetMessageHandler<CCILoginStep>(OnLoginStep)
            ;

            capi = api;
        }

        private void OnLoginStep(CCILoginStep step)
        {
            capi.ShowChatMessage("Login Step !");
            System.Diagnostics.Process.Start($"{step.url}");
        }

        private void OnRequestResponse(CCIRequestResponse response)
        {
            capi.ShowChatMessage($"response for request: {response.requestType} = {{succes:{response.success} , response:{response.response}}}");
        }
        #endregion
    }
}
