namespace vscci.GUI
{
    using Vintagestory.API.Client;

    using vscci.GUI.Elements;

    public class CCIEventDialogGui : GuiDialog
    {
        public override string ToggleKeyCombinationCode => "ccievent";
        public override string DebugName => "ccieventgui";

        public override float ZSize => 3000;

        public CCIEventDialogGui(ICoreClientAPI capi) : base(capi)
        {
        }

        public override void OnOwnPlayerDataReceived()
        {
            base.OnOwnPlayerDataReceived();

            var dialogBounds = ElementBounds.Fixed(EnumDialogArea.CenterMiddle, 0, 0, 1200, 700)
                .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);
            var scriptAreaBounds = ElementBounds.Fixed(0, 32, 1200, 668);

            dialogBounds.WithChild(scriptAreaBounds);

            SingleComposer = capi.Gui.CreateCompo("ccievent", dialogBounds)
                .AddDialogTitleBarWithBg("CCI Event", () => TryClose(), CairoFont.WhiteSmallishText())
                .AddInteractiveElement(new EventScriptingArea(capi, scriptAreaBounds))
                .Compose();
        }

        public override bool TryOpen()
        {
            if (base.TryOpen())
            {
                OnOwnPlayerDataReceived();
                return true;
            }
            return false;
        }

        public override bool TryClose()
        {
            if (base.TryClose())
            {
                Dispose();
                return true;
            }
            return false;
        }
    }
}
