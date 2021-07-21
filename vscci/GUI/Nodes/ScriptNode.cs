namespace vscci.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using System.Collections.Generic;
    using vscci.Data;
    using Vintagestory.API.Common;

    public class ScriptNode : GuiElement
    {
        protected readonly List<ScriptNodeInput> inputs;
        protected readonly List<ScriptNodeOutput> outputs;

        private readonly Matrix nodeTransform;
        private readonly TextDrawUtil textUtil;
        private readonly CairoFont font;

        private bool needsSize;
        private bool isMoving;
        private ScriptNodePinBase activePin;
        private ScriptNodePinConnection activeConnection;

        public ScriptNodePinConnection ActiveConnection => activeConnection;

        public ScriptNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base(api, bounds)
        {
            textUtil = new TextDrawUtil();
            this.nodeTransform = nodeTransform;
            font = CairoFont.WhiteDetailText().WithFontSize(20);
            isMoving = false;
            activePin = null;
            activeConnection = null;

            inputs = new List<ScriptNodeInput>()
            {
                { new ScriptNodeInput(this, "exec", typeof(Exec)) },
                { new ScriptNodeInput(this, "value", typeof(string)) }
            };

            outputs = new List<ScriptNodeOutput>()
            {
                { new ScriptNodeOutput(this, "exec", 1, typeof(Exec)) },
                { new ScriptNodeOutput(this, "value", 1, typeof(string)) }
            };

            needsSize = true;
        }

        public virtual void OnRender(Context ctx, ImageSurface surface, float deltaTime)
        {
            var x = Bounds.drawX;
            var y = Bounds.drawY;

            nodeTransform.TransformPoint(ref x,ref y);

            ctx.SetSourceRGBA(GuiStyle.DialogDefaultBgColor[0], GuiStyle.DialogDefaultBgColor[1], GuiStyle.DialogDefaultBgColor[2], GuiStyle.DialogDefaultBgColor[3]);
            EmbossRoundRectangleElement(ctx, x, y, Bounds.InnerWidth, Bounds.InnerHeight);
            //RoundRectangle(ctx, x, y, Bounds.InnerWidth, Bounds.InnerHeight, GuiStyle.ElementBGRadius);
            ctx.Fill();

            ctx.SetSourceRGBA(1.0, 1.0, 1.0, 1.0); 
            ctx.Save();

            var startDrawX = x + (Constants.NODE_SCIPRT_DRAW_PADDING / 2.0);
            var startDrawY = y + (Constants.NODE_SCIPRT_DRAW_PADDING / 2.0);
            var bigestWidth = 0.0d;
            var bigestHeight = 0.0d;

            x = startDrawX;
            y = startDrawY;

            foreach (var input in inputs)
            {
                input.Render(x, y, textUtil, font, ctx, surface);
                y += input.Extents.Height + Constants.NODE_SCIPRT_TEXT_PADDING;
                bigestWidth = bigestWidth > input.Extents.Width ? bigestWidth : input.Extents.Width;
                bigestHeight = bigestHeight > ( y  - startDrawY ) ? bigestWidth : (y - startDrawY);
            }

            x += bigestWidth + Constants.NODE_SCIPRT_TEXT_PADDING * Scale;
            y = startDrawY;

            foreach (var output in outputs)
            {
                output.Render(x, y, textUtil, font, ctx, surface);
                y += output.Extents.Height + Constants.NODE_SCIPRT_TEXT_PADDING;
                bigestWidth = bigestWidth > (x + output.Extents.Width) - startDrawX ? bigestWidth : (x + output.Extents.Width) - startDrawX;
                bigestHeight = bigestHeight > (y - startDrawY) ? bigestWidth : (y - startDrawY);
            }

            activeConnection?.Render(ctx, surface);

            if (needsSize)
            {
                Bounds = Bounds.WithFixedSize(bigestWidth + Constants.NODE_SCIPRT_DRAW_PADDING, bigestHeight + Constants.NODE_SCIPRT_DRAW_PADDING);
                Bounds.CalcWorldBounds();
                needsSize = false;
            }
        }
        public bool MouseDown(double x, double y, EnumMouseButton button)
        {
            if(IsPositionInside((int)x, (int)y))
            {
                foreach (var input in inputs)
                {
                    if(input.PointIsWithinSelectionBounds(x, y))
                    {
                        if (button == EnumMouseButton.Middle)
                        {
                            var connection = input.TopConnection();
                            if (connection  != null)
                            {
                                connection.DisconnectAll();
                            }
                        }
                        else if (button == EnumMouseButton.Left)
                        {
                            activePin = input;
                            activeConnection = input.CanCreateConnection ? new ScriptNodePinConnection(input) : input.Connections[0];
                            activeConnection.DrawPoint.X = x - Bounds.ParentBounds.absX;
                            activeConnection.DrawPoint.Y = y - Bounds.ParentBounds.absY;
                            return true;
                        }
                    }
                }

                foreach (var output in outputs)
                {
                    if (output.PointIsWithinSelectionBounds(x, y))
                    {
                        if (button == EnumMouseButton.Middle)
                        {
                            var connection = output.TopConnection();
                            if (connection != null)
                            {
                                connection.DisconnectAll();
                            }
                        }
                        else if (button == EnumMouseButton.Left)
                        {
                            activePin = output;
                            activeConnection = output.CanCreateConnection ? new ScriptNodePinConnection(output) : output.Connections[0];
                            activeConnection.DrawPoint.X = x - Bounds.ParentBounds.absX;
                            activeConnection.DrawPoint.Y = y - Bounds.ParentBounds.absY;
                            return true;
                        }
                    }
                }

                isMoving = true;
                return true;
            }
            return false;
        }

        public bool MouseUp(double x, double y)
        {
            if(isMoving)
            {
                isMoving = false;

                foreach (var input in inputs)
                {
                    input.MarkDirty();
                }

                foreach(var output in outputs)
                {
                    output.MarkDirty();
                }
            }

            if(activePin != null && activeConnection != null)
            {
                activePin = null;
                if (activeConnection.IsConnected == false)
                {
                    activeConnection.DisconnectAll();
                }
                activeConnection = null;
            }

            return true;
        }

        public bool ConnectionWillConnecttPoint(ScriptNodePinConnection connection, double x, double y)
        {
            if(connection.NeedsInput)
            {
                foreach (var input in inputs)
                {
                    if(input.PointIsWithinSelectionBounds(x, y) && connection.Connect(input))
                    {
                        return true;
                    }
                }
            }

            if (connection.NeedsOutput)
            {
                foreach (var output in outputs)
                {
                    if (output.PointIsWithinSelectionBounds(x, y) && connection.Connect(output))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void MouseMove(double deltaX, double deltaY)
        {
            if (isMoving)
            {
                Bounds = Bounds.WithFixedOffset(deltaX, deltaY);
                Bounds.CalcWorldBounds();
            }
            else if (activePin != null && activeConnection != null)
            {
                activeConnection.DrawPoint.X += deltaX;
                activeConnection.DrawPoint.Y += deltaY;
            }
        }
    }
}
