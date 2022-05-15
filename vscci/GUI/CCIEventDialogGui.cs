namespace VSCCI.GUI
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using Vintagestory.API.Client;

    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes;
    using VSCCI.GUI.Pins;

    public class CCIEventDialogGui : GuiDialog
    {
        private EventScriptingArea scriptingArea;

        private bool needsKeyboardEvents;

        private readonly List<ScriptNode> allNodes;

        public override string ToggleKeyCombinationCode => "ccievent";
        public override string DebugName => "ccieventgui";

        public override bool ShouldReceiveKeyboardEvents() => needsKeyboardEvents;

        public CCIEventDialogGui(ICoreClientAPI capi) : base(capi)
        {
            needsKeyboardEvents = false;
            allNodes = new List<ScriptNode>();
            OnOwnPlayerDataReceived();
        }

        public override void OnOwnPlayerDataReceived()
        {
            base.OnOwnPlayerDataReceived();

            var dialogBounds = ElementBounds.Fixed(EnumDialogArea.CenterMiddle, 0, 0, 1200, 700)
            .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);
            var scriptAreaBounds = ElementBounds.Fixed(0, 32, 1200, 668);

            dialogBounds.WithChild(scriptAreaBounds);

            scriptingArea = new EventScriptingArea(capi, allNodes, scriptAreaBounds);

            SingleComposer = capi.Gui.CreateCompo("ccievent", dialogBounds)
                .AddDialogTitleBarWithBg("CCI Event", () => TryClose(), CairoFont.WhiteSmallishText())
                .AddInteractiveElement(scriptingArea)
                .Compose();
        }

        public void LoadFromFile()
        {
            try
            {
                ResetNodesForLoad();

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

        public override void OnKeyDown(KeyEvent args)
        {
            base.OnKeyDown(args);

            if (args.CtrlPressed && args.KeyCode == (int)GlKeys.S)
            {
                SaveToFile();
                args.Handled = true;
            }
        }

        public override bool TryOpen()
        {
            if (base.TryOpen())
            {
                needsKeyboardEvents = true;
                OnOwnPlayerDataReceived();
                return true;
            }
            return false;
        }

        public override bool TryClose()
        {
            SaveToFile();

            if (base.TryClose())
            {
                needsKeyboardEvents = false;
                Dispose();
                return true;
            }
            return false;
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (var node in allNodes)
            {
                node.Dispose();
            }
        }

        public void ResetNodesForLoad()
        {
            foreach (var node in allNodes)
            {
                node.RemoveAllConnections();
            }

            allNodes.Clear();
            ScriptNodePinConnectionManager.TheManage.Dispose();
        }
    }
}
