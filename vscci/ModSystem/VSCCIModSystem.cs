namespace vscci.ModSystem
{
    using vscci.CCIIntegrations.Twitch;
    using vscci.Data;

    using Vintagestory.API.Common;
    using Vintagestory.API.Server;
    using Vintagestory.API.Client;
    using Vintagestory.API.Datastructures;

    public class VSCCIModSystem : ModSystem
    {
        // server side variables
        private ICoreServerAPI sapi;

        // client side variables
        private ICoreClientAPI capi;
        private TwitchIntegration ti;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.Network.RegisterChannel(Constants.NETWORK_CHANNEL);
        }
        #region Server
        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            api.Network.GetChannel(Constants.NETWORK_CHANNEL)
            ;
            sapi = api;

            api.Event.SaveGameLoaded += OnServerGameLoad;
            api.Event.GameWorldSave += OnServerGameSave;
            api.Event.PlayerJoin += OnPlayerLogin;
            api.Event.PlayerLeave += OnPlayerLogout;
        }

        public override void Dispose()
        {
            if (capi != null) // only if client should we do this
            {
                ti.Reset();
            }

            base.Dispose();
        }

        private void OnServerGameLoad()
        {
            ConfigData.LoadConfig(sapi);
        }

        private void OnServerGameSave()
        {
        }

        private void OnPlayerLogin(IServerPlayer player)
        {
        }

        private void OnPlayerLogout(IServerPlayer player)
        {
        }

        #endregion

        #region Client
        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);

            api.Event.RegisterEventBusListener(OnEvent);

            capi = api;
            ti = new TwitchIntegration(capi);
            ti.OnLoginSuccess += OnLoginSuccess;

            api.Event.LevelFinalize += EventLevelFinalize;
            api.Event.LeftWorld += EventLeftWorld;
        }

        private void EventLeftWorld()
        {
            ti.Reset();
        }

        private void OnEvent(string eventName, ref EnumHandling handling, IAttribute data)
        {
            switch (eventName)
            {
                case Constants.CCI_EVENT_CONNECT_REQUEST:
                    handling = EnumHandling.PreventSubsequent;
                    ti.Connect();
                    break;
                case Constants.CCI_EVENT_LOGIN_REQUEST:
                    handling = EnumHandling.PreventSubsequent;
                    ti.StartSignInFlow();
                    break;
                default:
                    break;
            }
        }

        private void OnLoginSuccess(object sender, string token)
        {
            SaveDataUtil.SaveClientData(capi, token);
        }

        private void EventLevelFinalize()
        {
            string token = null;
            SaveDataUtil.LoadClientData(capi, ref token);
            if(token != null)
            {
                ti.SetAuthDataFromSaveData(token);
            }
        }

        #endregion
    }
}
