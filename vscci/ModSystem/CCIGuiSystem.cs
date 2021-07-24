namespace VSCCI.ModSystem
{
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;

    using VSCCI.Data;
    using VSCCI.GUI;
    using VSCCI.CCINetworkTypes;
    using Vintagestory.API.Datastructures;

    internal class CCIGuiSystem : ModSystem
    {
        private CCIConfigDialogGui configGui;
        private CCIEventDialogGui eventGui;
        private ICoreClientAPI api;


        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.Network.RegisterChannel(Constants.NETWORK_GUI_CHANNEL).
                RegisterMessageType(typeof(CCIServerEventStatusUpdate));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);

            configGui = new CCIConfigDialogGui(api);
            eventGui = new CCIEventDialogGui(api);

            this.api = api;

            api.RegisterCommand("vscci", "Interface to vscci", "config | event", OnVsCCICommand);

            api.Event.RegisterEventBusListener(OnEvent);
            api.Network.GetChannel(Constants.NETWORK_GUI_CHANNEL)
                .SetMessageHandler<CCIServerEventStatusUpdate>(OnServerUpdateMessage);
        }

        private void OnServerUpdateMessage(CCIServerEventStatusUpdate update)
        {
            configGui.UpdateGuiServerStatusText(update.status);
        }

        private void OnEvent(string eventName, ref EnumHandling handling, IAttribute data)
        {
            switch (eventName)
            {
                case Constants.CCI_EVENT_CONNECT_UPDATE:
                    var cu = data.GetValue() as CCIConnectionUpdate;
                    if (cu.type == CCIIntegrations.CCIType.Twitch)
                    {
                        configGui.UpdateGuiTwitchConnectionText(cu.status);
                    }
                    else if (cu.type == CCIIntegrations.CCIType.Streamlabs)
                    {
                        configGui.UpdateGuiStreamlabsConnectionText(cu.status);
                    }
                    break;
                case Constants.CCI_EVENT_LOGIN_UPDATE:
                    var cl = data.GetValue() as CCILoginUpdate;
                    configGui.UpdateGuiTwitchLoginText(cl.user, cl.id);
                    break;
                case Constants.CCI_EVENT_SERVER_UPDATE:
                    var su = data.GetValue() as CCIServerEventStatusUpdate;
                    configGui.UpdateGuiServerStatusText(su.status);
                    break;
                default:
                    break;
            }
        }

        private void OnVsCCICommand(int groupId, CmdArgs arg)
        {
            if(arg.Length == 0)
            {
                api.ShowChatMessage("Need at least one argument");
                return;
            }

            switch(arg[0])
            {
                case "config":
                    configGui.TryOpen();
                    break;
                case "event":
                    eventGui.TryOpen();
                    break;
                default:
                    api.ShowChatMessage("Invalid Arguments");
                    break;
            }
        }
    }
}
