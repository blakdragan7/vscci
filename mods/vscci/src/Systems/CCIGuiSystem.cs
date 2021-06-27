using Vintagestory.API.Client;
using Vintagestory.API.Common;
using vscci.src.GUI;

namespace vscci.src.Systems
{
    class CCIGuiSystem : ModSystem
    {
        public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

        public override void StartClientSide(ICoreClientAPI api)
        {
            CCIConfigDialogGui gui = new CCIConfigDialogGui(api);

            api.Input.RegisterHotKey("claimgui", "Open Claim GUI", GlKeys.L, HotkeyType.GUIOrOtherControls);

            api.Event.LevelFinalize += () =>
            {
                if (api.Settings.Bool["claimGui"])
                {
                    gui.TryOpen();
                }
            };

            api.Input.SetHotKeyHandler("claimgui", a =>
            {
                gui.Toggle();
                return true;
            });
        }
    }
}
