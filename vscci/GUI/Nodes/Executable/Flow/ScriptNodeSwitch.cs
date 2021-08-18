namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using System.IO;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes.Attributes;
    using VSCCI.GUI.Pins;

    [NodeData("Flow", "Switch")]
    [InputPin(typeof(Exec), 0)]
    [InputPin(typeof(DynamicType), 1)]
    [OutputPin(typeof(Exec), 0)]
    public class ScriptNodeSwitch : ExecutableScriptNode
    {
        private const int ADD_BUTTON_SIZE = 20;

        private ScriptNodeInput input;
        private GuiElementToggleButton addOptionButton;

        public ScriptNodeSwitch(ICoreClientAPI api, MatrixElementBounds bounds) : base("Switch", "Default", api, bounds)
        {
            input = new ScriptNodeInput(this, "Value", typeof(DynamicType));
            addOptionButton = new GuiElementToggleButton(api, "", "+", CairoFont.ButtonText(), AddNewOption, ElementBounds.Empty);
            inputs.Add(input);

            shouldAutoExecuteNext = false;
        }

        protected override void OnExecute()
        {
            var connection = input.TopConnection();
            if (connection != null)
            {
                dynamic t = input.GetInput();

                if (connection.ConnectionType == typeof(string))
                {
                    foreach (var output in outputs)
                    {
                        var option = output as ScriptNodeSwitchOutput;
                        if (option != null)
                        {
                            string value = option.GetText();
                            bool shouldExecute;

                            try
                            {
                                shouldExecute = value == t;
                            }
                            catch (System.Exception)
                            {
                                shouldExecute = false;
                            }

                            if (shouldExecute)
                            {
                                ExecuteOutput(output);
                                return;
                            }
                        }
                    }
                }
                else if (connection.ConnectionType == typeof(Number))
                {
                    foreach (var output in outputs)
                    {
                        var option = output as ScriptNodeSwitchOutput;
                        if (option != null)
                        {
                            Number value;
                            bool shouldExecute;

                            try
                            {
                                value = Number.Parse(option.GetText());
                                shouldExecute = value == t;
                            }
                            catch (System.Exception)
                            {
                                shouldExecute = false;
                            }

                            if (shouldExecute)
                            {
                                ExecuteOutput(output);
                                return;
                            }
                        }
                    }
                }
                else if (connection.ConnectionType == typeof(bool))
                {
                    foreach (var output in outputs)
                    {
                        var option = output as ScriptNodeSwitchOutput;
                        if (option != null)
                        {
                            bool value;
                            bool shouldExecute;

                            try
                            {
                                value = bool.Parse(option.GetText());
                                shouldExecute = value == t;
                            }
                            catch (System.Exception)
                            {
                                shouldExecute = false;
                            }

                            if (shouldExecute)
                            {
                                ExecuteOutput(output);
                                return;
                            }
                        }
                    }
                }
            }

            // execute default
            ExecuteOutput(outputs[0]);
        }

        public override string GetNodeDescription()
        {
            return "This executes based on which path is euqal to \"Value\". If none are, \"Default\" is executed";
        }

        public override void WrtiePinsToBytes(BinaryWriter writer)
        {
            writer.Write(outputs.Count);
            base.WrtiePinsToBytes(writer);
        }

        public override void ReadPinsFromBytes(BinaryReader reader)
        {
            var numOptions = reader.ReadInt32();

            for (var i = outputs.Count; i < numOptions; i++) 
            {
                outputs.Add(new ScriptNodeSwitchOutput(this, RemoveOption, ""));
            }

            base.ReadPinsFromBytes(reader);
        }

        public override void ComposeElements(Context ctxStatic, ImageSurface surface)
        {
            base.ComposeElements(ctxStatic, surface);
            addOptionButton.ComposeElements(ctxStatic, surface);
        }

        public override void RenderInteractiveElements(float deltaTime)
        {
            base.RenderInteractiveElements(deltaTime);
            addOptionButton.RenderInteractiveElements(deltaTime);
        }

        protected override void ComposeSizeAndOffsets(Context ctx, CairoFont font)
        {
            base.ComposeSizeAndOffsets(ctx, font);

            Bounds.ChildBounds.Remove(addOptionButton.Bounds);
            addOptionButton.Bounds = ElementBounds.Fixed(Bounds.fixedWidth - ADD_BUTTON_SIZE, Bounds.fixedHeight, ADD_BUTTON_SIZE, ADD_BUTTON_SIZE);
            Bounds.WithChild(addOptionButton.Bounds);
            Bounds.WithFixedHeight(Bounds.fixedHeight + ADD_BUTTON_SIZE + 8);

            addOptionButton.Bounds.CalcWorldBounds();
            Bounds.CalcWorldBounds();
        }

        public override void OnMouseDown(ICoreClientAPI api, NodeMouseEvent @event)
        {
            addOptionButton.OnMouseDown(api, @event.mouseEvent);
            if (@event.mouseEvent.Handled == false)
                base.OnMouseDown(api, @event);
        }

        public override void OnMouseUp(ICoreClientAPI api, NodeMouseEvent @event)
        {
            addOptionButton.OnMouseUp(api, @event.mouseEvent);
            if (@event.mouseEvent.Handled == false)
                base.OnMouseUp(api, @event);
        }

        public override void OnMouseMove(ICoreClientAPI api, MouseEvent mouse)
        {
            addOptionButton.OnMouseUp(api, mouse);
            if (mouse.Handled == false)
                base.OnMouseMove(api, mouse);
        }

        private void AddNewOption(bool _)
        {
            api.Event.EnqueueMainThreadTask(() =>
            {
                outputs.Add(new ScriptNodeSwitchOutput(this, RemoveOption, ""));
                MarkDirty();
            }, "Add Switch Option");
        }

        private void RemoveOption(ScriptNodeSwitchOutput output)
        {
            api.Event.EnqueueMainThreadTask(() =>
            {
                output.Dispose();
                outputs.Remove(output);
                MarkDirty();
            }, "Remove Switch Option");
        }
    }
}
