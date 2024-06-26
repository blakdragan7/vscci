using System;

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
            CommandArgumentParsers parsers = api.ChatCommands.Parsers;
            api.ChatCommands.GetOrCreate("vscci").WithDescription("Interface to vscci")
                .RequiresPlayer()
                .WithArgs(parsers.Word("command", 
                    new string[]
                    {
                        "config", "event", "import", "export"

                    }), new StringArgParser("path", false))
                .HandleWith(OnVsCCICommand);

            api.Event.RegisterEventBusListener(OnEvent);
            api.Network.GetChannel(Constants.NETWORK_GUI_CHANNEL)
                .SetMessageHandler<CCIServerEventStatusUpdate>(OnServerUpdateMessage);

            eventGui.LoadFromFile();
        }

        private void OnServerUpdateMessage(CCIServerEventStatusUpdate update)
        {
            configGui.UpdateGuiServerStatusText(update.status);
            ConfigData.clientData.PlayerIsAllowedServerEvents = update.isAllowed;
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
                    else
                    {
                        configGui.UpdateGuiPlatformConnectionText(cu.status);
                        configGui.UpdatePlatfformSelection(cu.type);
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

        private TextCommandResult OnVsCCICommand(TextCommandCallingArgs arg)
        {
            if(arg.ArgCount == 0)
            {
                return TextCommandResult.Error("Invalid Arguments, usage .vscci {config | event | export | import}");
            }

            switch(arg[0] as string)
            {
                case "config":
                    configGui.TryOpen();
                    break;
                case "event":
                    eventGui.TryOpen();
                    break;
                case "export":
                    if (arg.ArgCount != 2)
                    {
                        api.ShowChatMessage("Invalid arguments for export, usage .vscci export file_path");
                    }

                    if (eventGui.Export(arg[1] as string))
                    {
                        api.ShowChatMessage("Successfully exported " + arg[1]);
                    }
                    else
                    {
                        api.ShowChatMessage("Failed to export " + arg[1]);
                    }

                    break;
                case "import":
                    if (arg.ArgCount != 2)
                    {
                        api.ShowChatMessage("Invalid arguments for export, usage .vscci export file_path");
                    }

                    if (eventGui.Import(arg[1] as string))
                    {
                        api.ShowChatMessage("Successfully imported " + arg[1]);
                    }
                    else
                    {
                        api.ShowChatMessage("Failed to import " + arg[1]);
                    }

                    break;
                default:
                    api.ShowChatMessage("Invalid Arguments, usage .vscci {config | event | export | import}");
                    break;
            }

            return TextCommandResult.Success();
        }
    }
}
