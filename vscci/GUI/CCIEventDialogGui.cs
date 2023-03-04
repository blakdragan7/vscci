namespace VSCCI.GUI
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Windows.Forms;
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
            var importBounds = ElementBounds.Fixed(1140-60, 644, 50, 10);
            var exportBounds = ElementBounds.Fixed(1140, 644, 50, 10);

            scriptAreaBounds.WithChildren(importBounds, exportBounds);
            dialogBounds.WithChild(scriptAreaBounds);

            scriptingArea = new EventScriptingArea(capi, allNodes, scriptAreaBounds);

            SingleComposer = capi.Gui.CreateCompo("ccievent", dialogBounds)
                .AddDialogTitleBarWithBg("CCI Event", () => TryClose(), CairoFont.WhiteSmallishText())
                .AddButton("Import", OnImport, importBounds, CairoFont.WhiteSmallishText())
                .AddButton("Export", OnExport, exportBounds, CairoFont.WhiteSmallishText())
                .AddInteractiveElement(scriptingArea)
                .Compose();
        }

        public bool OnExport()
        {
            string path = "";

            using (SaveFileDialog openFileDialog = new SaveFileDialog())
            {
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                openFileDialog.Filter = "vscci save file | *.data";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    path = openFileDialog.FileName;
                    if (Path.GetExtension(path) != ".data")
                    {
                        path += ".data";
                    }

                    try
                    {
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
            }

            return true;
        }

        public bool OnImport()
        {

            string path = "";

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                openFileDialog.Filter = "vscci save file | *.data";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    path = openFileDialog.FileName;

                    try
                    {
                        ResetNodesForLoad();
                        ScriptNodePinConnectionManager.TheManage.SetupManager(capi, scriptingArea.Bounds);

                        using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
                        {
                            scriptingArea.FromBytes(reader, capi.World);
                        };

                        SaveToFile();
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
            }

            return true;
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
