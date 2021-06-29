using Vintagestory.API.Client;
using Vintagestory.API.Common;
using vscci.src.CCINetworkTypes;
using vscci.src.Data;
using vscci.src.GUI;

namespace vscci.src.Systems
{
    class CCIGuiSystem : ModSystem
    {
        public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;
        CCIConfigDialogGui gui;
        private ICoreClientAPI api;

        public override void StartClientSide(ICoreClientAPI api)
        {
            gui = new CCIConfigDialogGui(api);
            this.api = api;

            api.RegisterCommand("vscci", "Interface to vscci", "config", OnVsCCICommand);
            api.Network.GetChannel(Constants.NETWORK_CHANNEL)
                .SetMessageHandler<CCILoginUpdate>(OnLoginUpdate)
                .SetMessageHandler<CCIConnectionUpdate>(OnConnectUpdate)
            ;
        }

        private void OnLoginUpdate(CCILoginUpdate response)
        {
            gui.UpdateGuiLoginText(response.user, response.id);
        }

        private void OnConnectUpdate(CCIConnectionUpdate response)
        {
            gui.UpdateGuiConnectionText(response.status);
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
                    gui.TryOpen();
                    break;
                default:
                    api.ShowChatMessage("Invalid Arguments");
                    break;
            }
        }
    }
}
