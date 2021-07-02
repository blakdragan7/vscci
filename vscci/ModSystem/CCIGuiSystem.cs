namespace vscci.ModSystem
{
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;

    using vscci.Data;
    using vscci.GUI;
    using vscci.CCINetworkTypes;

    class CCIGuiSystem : ModSystem
    {
        CCIConfigDialogGui configGui;
        CCIEventDialogGui eventGui;
        private ICoreClientAPI api;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.Network.RegisterChannel(Constants.NETWORK_GUI_CHANNEL)
                .RegisterMessageType(typeof(CCILoginUpdate))
                .RegisterMessageType(typeof(CCIConnectionUpdate))
            ;
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            configGui = new CCIConfigDialogGui(api);
            eventGui = new CCIEventDialogGui(api);

            this.api = api;

            api.RegisterCommand("vscci", "Interface to vscci", "config", OnVsCCICommand);
            api.Network.GetChannel(Constants.NETWORK_GUI_CHANNEL)
                .SetMessageHandler<CCILoginUpdate>(OnLoginUpdate)
                .SetMessageHandler<CCIConnectionUpdate>(OnConnectUpdate)
            ;
        }

        private void OnLoginUpdate(CCILoginUpdate response)
        {
            configGui.UpdateGuiLoginText(response.user, response.id);
        }

        private void OnConnectUpdate(CCIConnectionUpdate response)
        {
            configGui.UpdateGuiConnectionText(response.status);
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
