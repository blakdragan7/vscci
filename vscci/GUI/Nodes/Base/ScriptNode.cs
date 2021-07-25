namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using System.Collections.Generic;
    using VSCCI.Data;
    using Vintagestory.API.Common;

    public class ScriptNode : GuiElement
    {
        protected readonly List<ScriptNodeInput> inputs;
        protected readonly List<ScriptNodeOutput> outputs;

        private readonly Matrix nodeTransform;
        private readonly TextDrawUtil textUtil;
        private readonly CairoFont font;

        private double cachedRenderX;
        private double cachedRenderY;

        private bool isDirty;
        private bool isMoving;
        private ScriptNodePinBase activePin;
        private ScriptNodePinConnection activeConnection;

        private string title;

        private TextExtents titleExtents;

        public bool IsDirty => isDirty;

        public ScriptNodePinConnection ActiveConnection => activeConnection;

        public ScriptNode(string _title, ICoreClientAPI api, Matrix _nodeTransform, ElementBounds bounds) : base(api, bounds)
        {
            nodeTransform = _nodeTransform;

            textUtil = new TextDrawUtil();
            font = CairoFont.WhiteDetailText().WithFontSize(20);

            isMoving = false;
            activePin = null;
            activeConnection = null;
            title = _title;

            inputs = new List<ScriptNodeInput>();
            outputs = new List<ScriptNodeOutput>();

            isDirty = true;
            titleExtents = new TextExtents();

            cachedRenderX = 0;
            cachedRenderY = 0;
        }

        public virtual void OnRender(Context ctx, ImageSurface surface, float deltaTime)
        {
            if (isDirty)
            {
                OnCompose(ctx, font);
            }

            var x = cachedRenderX;
            var y = cachedRenderY;

            // Draw Title Background
            if (title.Length > 0)
            {
                ctx.SetSourceRGBA(GuiStyle.DialogDefaultBgColor[0], GuiStyle.DialogDefaultBgColor[1], GuiStyle.DialogDefaultBgColor[2], GuiStyle.DialogDefaultBgColor[3]);
                RoundRectangle(ctx, x, y, Bounds.InnerWidth, titleExtents.Height, 1);
                ctx.Fill();

                EmbossRoundRectangleElement(ctx, x, y, Bounds.InnerWidth, titleExtents.Height);
            }
            // Draw Pin Background
            ctx.SetSourceRGBA(GuiStyle.DialogDefaultBgColor[0], GuiStyle.DialogDefaultBgColor[1], GuiStyle.DialogDefaultBgColor[2], GuiStyle.DialogDefaultBgColor[3]);
            RoundRectangle(ctx, x, y, Bounds.InnerWidth, Bounds.InnerHeight, 1);
            ctx.Fill();
            
            EmbossRoundRectangleElement(ctx, x, y + titleExtents.Height, Bounds.InnerWidth, Bounds.InnerHeight - titleExtents.Height);

            foreach (var input in inputs)
            {
                input.RenderOther(ctx, surface);
            }

            foreach (var output in outputs)
            {
                output.RenderOther(ctx, surface);
            }

            ctx.Save();

            ctx.SetSourceRGBA(1.0, 1.0, 1.0, 1.0);
            font.SetupContext(ctx);
            if (title.Length > 0)
            {
                textUtil.DrawTextLine(ctx, font, title, x + (Bounds.InnerWidth / 2.0) - (titleExtents.Width / 2.0), y);
            }

            foreach (var input in inputs)
            {
                input.RenderText(textUtil, font, ctx, surface);
            }

            foreach (var output in outputs)
            {
                output.RenderText(textUtil, font, ctx, surface);
            }

            ctx.Restore();

            foreach (var input in inputs)
            {
                input.RenderPin(ctx, surface);
            }

            foreach (var output in outputs)
            {
                output.RenderPin(ctx, surface);
            }

            activeConnection?.Render(ctx, surface);
        }

        protected virtual void OnCompose(Context ctx, CairoFont font)
        {
            cachedRenderX = Bounds.drawX;
            cachedRenderY = Bounds.drawY;

            nodeTransform.TransformPoint(ref cachedRenderX, ref cachedRenderY);

            var x = cachedRenderX;
            var y = cachedRenderY;

            ctx.Save();
            font.SetupContext(ctx);

            if (title.Length > 0)
            {
                titleExtents = ctx.TextExtents(title);
                titleExtents.Height += 4;
            }
            else
            {
                titleExtents = new TextExtents();
                titleExtents.Width = 0;
                titleExtents.Height = 0;
            }

            var startDrawX = x + (Constants.NODE_SCIPRT_DRAW_PADDING / 2.0);
            var startDrawY = y + titleExtents.Height + (Constants.NODE_SCIPRT_DRAW_PADDING / 2.0);
            var bigestWidth = titleExtents.Width;
            var bigestHeight = 0.0d;

            x = startDrawX;
            y = startDrawY;

            foreach (var input in inputs)
            {
                input.Compose(x, y, ctx, font);
                y += input.Extents.Height + Constants.NODE_SCIPRT_TEXT_PADDING * Scale;
                bigestWidth = bigestWidth > input.Extents.Width ? bigestWidth : input.Extents.Width;
                bigestHeight = bigestHeight > (y - startDrawY) ? bigestHeight : (y - startDrawY);
            }

            x += bigestWidth + Constants.NODE_SCIPRT_TEXT_PADDING * Scale;
            y = startDrawY;

            foreach (var output in outputs)
            {
                output.Compose(x, y, ctx, font);
                y += output.Extents.Height + Constants.NODE_SCIPRT_TEXT_PADDING * Scale;
                bigestWidth = bigestWidth > (x + output.Extents.Width) - startDrawX ? bigestWidth : (x + output.Extents.Width) - startDrawX;
                bigestHeight = bigestHeight > (y - startDrawY) ? bigestHeight : (y - startDrawY);
            }

            ctx.Restore();

            Bounds = Bounds.WithFixedSize(bigestWidth + (Constants.NODE_SCIPRT_DRAW_PADDING / 2.0), bigestHeight + titleExtents.Height + (Constants.NODE_SCIPRT_DRAW_PADDING / 2.0));
            Bounds.CalcWorldBounds();

            isDirty = false;
        }

        public virtual bool MouseDown(double x, double y, EnumMouseButton button)
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

        public virtual bool MouseUp(double x, double y)
        {
            if(isMoving)
            {
                isMoving = false;
            }
            else if(activePin != null && activeConnection != null)
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

        public void MouseMove(double deltaX, double deltaY)
        {
            if (isMoving)
            {
                Bounds = Bounds.WithFixedOffset(deltaX, deltaY);
                Bounds.CalcWorldBounds();

                isDirty = true;
            }
            else if (activePin != null && activeConnection != null)
            {
                activeConnection.DrawPoint.X += deltaX;
                activeConnection.DrawPoint.Y += deltaY;
            }
        }

        public bool ConnectionWillConnecttPoint(ScriptNodePinConnection connection, double x, double y)
        {
            if (connection.NeedsInput)
            {
                foreach (var input in inputs)
                {
                    if (input.PointIsWithinSelectionBounds(x, y) && connection.Connect(input))
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
    }
}
