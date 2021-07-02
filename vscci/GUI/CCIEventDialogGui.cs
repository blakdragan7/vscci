namespace vscci.GUI
{
    using System;
    using Vintagestory.API.Client;

    public class CCIEventDialogGui : GuiDialog
    {
        public override string ToggleKeyCombinationCode => "ccievent";

        public override float ZSize => 3000;

        private readonly ICoreClientAPI api;
        public CCIEventDialogGui(ICoreClientAPI capi) : base(capi)
        {
            api = capi;
        }

        public override void OnOwnPlayerDataReceived()
        {
            base.OnOwnPlayerDataReceived();

            ElementBounds dialogBounds = ElementBounds.Fixed(EnumDialogArea.CenterMiddle, 0, 0, 1200, 700)
                .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);
            ElementBounds bgBounds = dialogBounds.CopyOffsetedSibling(0, 0).WithFixedPadding(GuiStyle.DialogToScreenPadding);

            SingleComposer = capi.Gui.CreateCompo("ccievent", dialogBounds)
                .AddDialogTitleBarWithBg("CCI Event", () => TryClose(), CairoFont.WhiteSmallishText())
                .AddGrayBG(bgBounds)
                .AddStaticCustomDraw(dialogBounds.CopyOffsetedSibling(), OnCustomRender)
                .Compose();
        }

        private void OnCustomRender(Cairo.Context ctx, Cairo.ImageSurface surface, ElementBounds currentBounds)
        {
            if (ctx is null)
            {
                throw new ArgumentNullException(nameof(ctx));
            }

            if (surface is null)
            {
                throw new ArgumentNullException(nameof(surface));
            }
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
