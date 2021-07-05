namespace vscci.ModSystem
{
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;

    using vscci.Data;
    using vscci.GUI;
    using vscci.CCINetworkTypes;
    using Vintagestory.API.Datastructures;

    class CCIGuiSystem : ModSystem
    {
        private CCIConfigDialogGui configGui;
        private CCIEventDialogGui eventGui;
        private ICoreClientAPI api;

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide == EnumAppSide.Client;
        }
        public override void StartClientSide(ICoreClientAPI api)
        {
            configGui = new CCIConfigDialogGui(api);
            eventGui = new CCIEventDialogGui(api);

            this.api = api;

            api.RegisterCommand("vscci", "Interface to vscci", "config | event", OnVsCCICommand);

            api.Event.RegisterEventBusListener(OnEvent);
        }

        private void OnEvent(string eventName, ref EnumHandling handling, IAttribute data)
        {
            switch (eventName)
            {
                case Constants.CCI_EVENT_CONNECT_UPDATE:
                    var cu = data.GetValue() as CCIConnectionUpdate;
                    configGui.UpdateGuiConnectionText(cu.status);
                    break;
                case Constants.CCI_EVENT_LOGIN_UPDATE:
                    var cl = data.GetValue() as CCILoginUpdate;
                    configGui.UpdateGuiLoginText(cl.user, cl.id);
                    break;
            }
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
