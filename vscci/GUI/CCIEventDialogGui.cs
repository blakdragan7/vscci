namespace VSCCI.GUI
{
    using System;
    using System.IO;
    using Vintagestory.API.Client;

    using VSCCI.GUI.Elements;

    public class CCIEventDialogGui : GuiDialog
    {
        private EventScriptingArea scriptingArea;

        public override string ToggleKeyCombinationCode => "ccievent";
        public override string DebugName => "ccieventgui";

        //public override float ZSize => 3000;


        public CCIEventDialogGui(ICoreClientAPI capi) : base(capi)
        {
            OnOwnPlayerDataReceived();
        }

        public override void OnOwnPlayerDataReceived()
        {
            base.OnOwnPlayerDataReceived();

            var dialogBounds = ElementBounds.Fixed(EnumDialogArea.CenterMiddle, 0, 0, 1200, 700)
            .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);
            var scriptAreaBounds = ElementBounds.Fixed(0, 32, 1200, 668);

            dialogBounds.WithChild(scriptAreaBounds);

            scriptingArea = new EventScriptingArea(capi, scriptAreaBounds);

            SingleComposer = capi.Gui.CreateCompo("ccievent", dialogBounds)
                .AddDialogTitleBarWithBg("CCI Event", () => TryClose(), CairoFont.WhiteSmallishText())
                .AddInteractiveElement(scriptingArea)
                .Compose();

            LoadFromFile();
        }

        public void LoadFromFile()
        {
            try
            {
                var path = "vscci_event_" + capi.World.SavegameIdentifier + ".data";
                using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
                {
                    scriptingArea.FromBytes(reader, capi.World);
                };
            }
            catch (FileNotFoundException)
            {
                // skip
            }
            catch (Exception exc)
            {
                capi.Logger.Error("Error Loading Save File {0}", exc.Message);
            }
        }

        public void SaveToFile()
        {
            try
            {
                var path = "vscci_event_" + capi.World.SavegameIdentifier + ".data";
                FileMode mode = File.Exists(path) ? FileMode.Truncate : FileMode.Create;
                using (BinaryWriter writer = new BinaryWriter(File.Open(path, mode, FileAccess.Write)))
                {
                    scriptingArea.ToBytes(writer);
                };
            }
            catch (FileNotFoundException)
            {
                // skip
            }
            catch (Exception exc)
            {
                capi.Logger.Error("Error Loading Save File {0}", exc.Message);
            }
        }

        public override void OnKeyPress(KeyEvent args)
        {
            base.OnKeyPress(args);

            if (args.CtrlPressed && args.KeyCode == (int)GlKeys.S)
            {
                SaveToFile();
            }
        }

        public override bool TryOpen()
        {
            if (base.TryOpen())
            {
                if(SingleComposer == null)OnOwnPlayerDataReceived();
                return true;
            }
            return false;
        }

        public override bool TryClose()
        {
            SaveToFile();

            if (base.TryClose())
            {
                return true;
            }
            return false;
        }
    }
}
