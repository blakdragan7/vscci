using Vintagestory.API.Client;
using vscci.src.CCINetworkTypes;
using vscci.src.Data;

namespace vscci.src.GUI
{
    class CCIConfigDialogGui : GuiDialog
    {
        public override string ToggleKeyCombinationCode => "ccigui";

        public CCIConfigDialogGui(ICoreClientAPI capi) : base(capi)
        {
            this.capi = capi;
        }

        public override void OnOwnPlayerDataReceived()
        {
            base.OnOwnPlayerDataReceived();

            ElementBounds radialRoot = ElementBounds.Fixed(EnumDialogArea.LeftMiddle, 0, 0, 25, 25);
            ElementBounds dialogBounds = radialRoot.CopyOffsetedSibling(0, 0, 600, 200);
            ElementBounds bgBounds = dialogBounds.CopyOffsetedSibling();
            radialRoot = radialRoot.WithFixedOffset(165, 15);

            SingleComposer = capi.Gui.CreateCompo("cciconfig", dialogBounds)
                .AddDialogTitleBar("cci config", () => TryClose(), CairoFont.WhiteSmallText())
                .AddDialogBG(bgBounds)
                .AddTextToggleButtons(new string[] { "Connect", "Login"}, CairoFont.ButtonText().WithFontSize(10),
                i =>
                {
                    switch (i)
                    {
                        case 0:
                            capi.Network.GetChannel(Constants.NETWORK_CHANNEL).SendPacket<CCIConnectRequest>(new CCIConnectRequest() {twitchid="",istwitchpartner=false });
                            break;
                        case 1:
                            capi.Network.GetChannel(Constants.NETWORK_CHANNEL).SendPacket<CCILoginRequest>(new CCILoginRequest() { });
                            break;
                        default:
                            break;
                    }
                    capi.Event.RegisterCallback(dt => SingleComposer.GetToggleButton("buttons-" + i).On = false, 50);
                },
                new ElementBounds[]
                {
                    radialRoot.CopyOffsetedSibling(-150, -25, 25),
                    radialRoot.CopyOffsetedSibling(-150, 0, 25),
                    radialRoot,
                }, "buttons")
                .AddToggleButton("Close", CairoFont.ButtonText().WithFontSize(10),
                b =>
                {
                    TryClose();
                }, radialRoot.CopyOffsetedSibling(-150,25,25))
                .Compose();
        }

        public override bool TryOpen()
        {
            if (base.TryOpen())
            {
                OnOwnPlayerDataReceived();
                capi.Settings.Bool["ccigui"] = true;
                return true;
            }
            return false;
        }

        public override bool TryClose()
        {
            if (base.TryClose())
            {
                capi.Settings.Bool["ccigui"] = false;
                Dispose();
                return true;
            }
            return false;
        }
    }
}
