namespace vscci.ModSystem
{
    using System.Collections.Generic;

    using vscci.CCIIntegrations.Twitch;
    using vscci.CCINetworkTypes;
    using vscci.Data;

    using Vintagestory.API.Common;
    using Vintagestory.API.Server;
    using Vintagestory.API.Client;

    class VSCCIModSystem : ModSystem
    {
        // server side variables
        private Dictionary<string, string> cachedPlayerData;
        private Dictionary<IServerPlayer, TwitchIntegration> dti;
        private ICoreServerAPI sapi;

        // client side variables
        private ICoreClientAPI capi;
        private TwitchAuthenticationHelperClient tiClient;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.Network.RegisterChannel(Constants.NETWORK_CHANNEL)
                .RegisterMessageType(typeof(TwitchLoginStep))
                .RegisterMessageType(typeof(TwitchLoginStepResponse))
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
            api.Event.PlayerJoin += OnPlayerLogin;
            api.Event.PlayerLeave += OnPlayerLogout;
        }

        public override void Dispose()
        {
            if (sapi != null) // only if server should we do this
            {
                foreach (var pair in dti)
                {
                    pair.Value.Reset();
                }
            }

            base.Dispose();
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
            SaveDataUtil.LoadAuthData(sapi, this, ref cachedPlayerData);
        }

        private void OnGameSave()
        {
            SaveDataUtil.SaveAuthData(sapi, dti, cachedPlayerData);
        }

        private void OnPlayerLogin(IServerPlayer player)
        {
            var ti = TIForPlayer(player);

            string oauth;
            if(cachedPlayerData.TryGetValue(player.PlayerUID, out oauth))
            {
                ti.SetAuthDataFromSaveData(oauth);
                cachedPlayerData.Remove(player.PlayerUID);
            }
        }

        private void OnPlayerLogout(IServerPlayer player)
        {
            TIForPlayer(player).Reset();
        }

        private void OnCCILoginRequest(IPlayer fromPlayer, CCILoginRequest request)
        {
            TIForPlayer(fromPlayer as IServerPlayer).StartSignInFlow();
        }

        private void OnCCIConnectRequest(IPlayer fromPlayer, CCIConnectRequest request)
        {
            TIForPlayer(fromPlayer as IServerPlayer).Connect();
        }

        private void OnCCILogin(object sender, IServerPlayer player)
        {
            sapi.Network.GetChannel(Constants.NETWORK_CHANNEL).SendPacket<CCIRequestResponse>(new CCIRequestResponse() { requestType = "login", response = $"success", success = true }, new[] { player });
        }

        private void OnCCILoginFailed(object sender, OnAuthFailedArgs args)
        {
            sapi.Network.GetChannel(Constants.NETWORK_CHANNEL).SendPacket<CCIRequestResponse>(new CCIRequestResponse() { requestType = "login", response = args.Message, success = false }, new[] { args.Player });
        }

        private void OnCCIConnect(object sender, IServerPlayer player)
        {
            sapi.Network.GetChannel(Constants.NETWORK_CHANNEL).SendPacket<CCIRequestResponse>(new CCIRequestResponse() { requestType = "connect", response = $"success", success = true }, new[] { player });
        }

        private void OnCCIConnectFailed(object sender, OnConnectFailedArgs args)
        {
            sapi.Network.GetChannel(Constants.NETWORK_CHANNEL).SendPacket<CCIRequestResponse>(new CCIRequestResponse() { requestType = "connect", response = args.Reason, success = false }, new[] { args.Player });
        }
        #endregion

        #region Client
        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);

            api.Network.GetChannel(Constants.NETWORK_CHANNEL)
                .SetMessageHandler<CCIRequestResponse>(OnRequestResponse)
            ;

            capi = api;

            tiClient = new TwitchAuthenticationHelperClient(capi);
        }

        private void OnRequestResponse(CCIRequestResponse response)
        {
            capi.ShowChatMessage($"response for request: {response.requestType} = {{succes:{response.success} , response:{response.response}}}");
        }
        #endregion
    }
}
