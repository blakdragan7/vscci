namespace VSCCI.ModSystem
{
    using VSCCI.CCINetworkTypes;
    using VSCCI.CCIIntegrations;
    using VSCCI.CCIIntegrations.Twitch;
    using VSCCI.CCIIntegrations.Streamlabs;
    using VSCCI.CCIIntegrations.Streamelements;
    using VSCCI.Data;

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
        private SteamLabsIntegration si;
        private StreamelementsIntegration se;

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
                si.Reset();
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
            var isAllowed = ConfigData.PlayerIsAllowed(player);
            var status = isAllowed ? "All-Allowed" : "Client-Only";

            sapi.Network.GetChannel(Constants.NETWORK_GUI_CHANNEL).SendPacket(new CCIServerEventStatusUpdate() 
            {
                status = status, 
                isAllowed = isAllowed 
            }, player);
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
            ti.OnLoginSuccess += OnTwitchLoginSuccess;

            si = new SteamLabsIntegration(api);
            si.OnLoginSuccess += OnStreamlabsLoginSuccess;

            se = new StreamelementsIntegration(api);
            se.OnLoginSuccess += OnStreamelementsLoginSuccess;

            api.Event.LevelFinalize += EventLevelFinalize;
            api.Event.LeftWorld += EventLeftWorld;
        }

        private void EventLeftWorld()
        {
            ti.Reset();
            si.Reset();
        }

        private void OnEvent(string eventName, ref EnumHandling handling, IAttribute data)
        {
            switch (eventName)
            {
                case Constants.CCI_EVENT_DISCONNECT_REQUEST:
                    handling = EnumHandling.PreventSubsequent;
                    var dc = data.GetValue() as CCIDisconnectRequest;
                    if (dc != null)
                    {
                        var saveData = SaveDataUtil.LoadClientData(capi);

                        switch (dc.type)
                        {
                            case CCIType.Twitch:
                                ti.Reset();
                                saveData.TwitchAuth = "";
                                break;
                            case CCIType.Streamlabs:
                                si.Reset();
                                saveData.PlatformType = CCIType.Twitch;
                                saveData.PlatformAuth = "";
                                break;
                            case CCIType.Streamelements:
                                se.Reset();
                                saveData.PlatformType = CCIType.Twitch;
                                saveData.PlatformAuth = "";
                                break;
                            default:
                                break;
                        }

                        SaveDataUtil.SaveClientData(capi, saveData);
                    }
            
                    break;
                case Constants.CCI_EVENT_LOGIN_REQUEST:
                    var ld = data.GetValue() as CCILoginRequest;
                    foreach (var type in ld.type)
                    {
                        switch (type)
                        {
                            case CCIType.Twitch:
                                handling = EnumHandling.PreventSubsequent;
                                ti.StartSignInFlow();
                                break;
                            case CCIType.Streamlabs:
                                se.SetRawAuthData("");
                                si.SetRawAuthData(ld.data);
                                si.Connect();
                                break;
                            case CCIType.Streamelements:
                                si.SetRawAuthData("");
                                se.SetRawAuthData(ld.data);
                                se.Connect();
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void OnStreamelementsLoginSuccess(object sender, string token)
        {
            if (token != null)
            {
                var data = new ClientSaveData() 
                { 
                    TwitchAuth = ti.GetAuthDataForSaving(), 
                    PlatformType = CCIType.Streamelements, 
                    PlatformAuth = token 
                };
                SaveDataUtil.SaveClientData(capi, data);
            }
        }

        private void OnStreamlabsLoginSuccess(object sender, string token)
        {
            if (token != null)
            {
                var data = new ClientSaveData()
                {
                    TwitchAuth = ti.GetAuthDataForSaving(),
                    PlatformType = CCIType.Streamlabs,
                    PlatformAuth = token
                };
                SaveDataUtil.SaveClientData(capi, data);
            }
        }

        private void OnTwitchLoginSuccess(object sender, string token)
        {
            // if token is null this was received from save file, so no reason to save again
            if (token != null)
            {
                var data = SaveDataUtil.LoadClientData(capi);

                data.TwitchAuth = ti.GetAuthDataForSaving();

                if (se.IsConnected())
                {
                    data.PlatformType = CCIType.Streamelements;
                    data.PlatformAuth = se.GetAuthDataForSaving();
                }
                else if(si.IsConnected())
                {
                    data.PlatformType = CCIType.Streamlabs;
                    data.PlatformAuth = se.GetAuthDataForSaving();
                }
                else
                {
                    data.PlatformType = CCIType.Twitch;
                    data.PlatformAuth = "";
                }

                SaveDataUtil.SaveClientData(capi, data);
            }
            ti.Connect();
        }

        private void EventLevelFinalize()
        {
            var data = SaveDataUtil.LoadClientData(capi);
            if(data != null)
            {
                if (data.TwitchAuth != null && data.TwitchAuth.Length > 0)
                {
                    ti.SetAuthDataFromSaveData(data.TwitchAuth);
                }
                switch(data.PlatformType)
                {
                    case CCIType.Streamlabs:
                        if(data.PlatformAuth.Length > 0)
                            si.SetAuthDataFromSaveData(data.PlatformAuth);
                        break;
                    case CCIType.Streamelements:
                        if(data.PlatformAuth.Length > 0)
                            se.SetAuthDataFromSaveData(data.PlatformAuth);
                        break;
                }
            }
        }

        #endregion
    }
}
