namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using System.Collections.Generic;
    using VSCCI.Data;
    using Vintagestory.API.Common;
    using System;
    using System.IO;

    public enum ScriptNodeState
    {
        Focused,
        Dragged,
        Selected,
        PinSelected,
        None
    }

    public abstract class ScriptNode : GuiElement
    {
        protected readonly List<ScriptNodeInput> inputs;
        protected readonly List<ScriptNodeOutput> outputs;

        protected readonly Matrix nodeTransform;
        protected double cachedRenderX;
        protected double cachedRenderY;
        protected bool isDirty;

        protected ScriptNodeState state;

        private readonly TextDrawUtil textUtil;
        private readonly CairoFont font;

        private ScriptNodePinBase activePin;
        private ScriptNodePinConnection activeConnection;

        private string title;

        private TextExtents titleExtents;

        public bool IsDirty => isDirty;

        public Guid Guid;

        public ScriptNodePinConnection ActiveConnection => activeConnection;

        public ScriptNode(string _title, ICoreClientAPI api, Matrix _nodeTransform, ElementBounds bounds) : base(api, bounds)
        {
            nodeTransform = _nodeTransform;

            textUtil = new TextDrawUtil();
            font = CairoFont.WhiteDetailText().WithFontSize(20);

            state = ScriptNodeState.None;

            activePin = null;
            activeConnection = null;
            title = _title;

            inputs = new List<ScriptNodeInput>();
            outputs = new List<ScriptNodeOutput>();

            isDirty = true;
            titleExtents = new TextExtents();

            cachedRenderX = 0;
            cachedRenderY = 0;

            Guid = Guid.NewGuid();
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
                input.RenderOther(ctx, surface, deltaTime);
            }

            foreach (var output in outputs)
            {
                output.RenderOther(ctx, surface, deltaTime);
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
                input.RenderText(textUtil, font, ctx, surface, deltaTime);
            }

            foreach (var output in outputs)
            {
                output.RenderText(textUtil, font, ctx, surface, deltaTime);
            }

            ctx.Restore();

            foreach (var input in inputs)
            {
                input.RenderPin(ctx, surface, deltaTime);
            }

            foreach (var output in outputs)
            {
                output.RenderPin(ctx, surface, deltaTime);
            }

            // Draw selected highlight
            if (state == ScriptNodeState.Selected)
            {
                ctx.SetSourceRGBA(1.0, 1.0, 1.0, 0.3);
                RoundRectangle(ctx, x, y, Bounds.OuterWidth, Bounds.OuterHeight, 1);
                ctx.Fill();
            }

            activeConnection?.Render(ctx, surface);
        }

        public override void RenderInteractiveElements(float deltaTime)
        {
            base.RenderInteractiveElements(deltaTime);

            foreach (var input in inputs)
            {
                input.RenderInteractive(deltaTime);
            }

            foreach (var output in outputs)
            {
                output.RenderInteractive(deltaTime);
            }
        }

        public void AddConnectionsToList(List<ScriptNodePinConnection> connections)
        {
            foreach (var pin in inputs)
            {
                pin.AddConnectionsToList(connections);
            }

            foreach (var pin in outputs)
            {
                pin.AddConnectionsToList(connections);
            }
        }

        public ScriptNodeInput InputForGuid(Guid guid)
        {
            foreach(var input in inputs)
            {
                if (input.Guid == guid)
                    return input;
            }

            return null;
        }

        public ScriptNodeOutput OutputForGuid(Guid guid)
        {
            foreach (var output in outputs)
            {
                if (output.Guid == guid)
                    return output;
            }

            return null;
        }

        public ScriptNodeOutput OutputForIndex(int index)
        {
            if (index >= outputs.Count)
                return null;

            return outputs[index];
        }

        public ScriptNodeInput InputForIndex(int index)
        {
            if (index >= inputs.Count)
                return null;

            return inputs[index];
        }

        public void ReadPinsFromBytes(BinaryReader reader)
        {
            foreach (var pin in inputs)
            {
                pin.FromBytes(reader);
            }

            foreach (var pin in outputs)
            {
                pin.FromBytes(reader);
            }

            isDirty = true;
        }

        public void WrtiePinsToBytes(BinaryWriter writer)
        {
            foreach(var pin in inputs)
            {
                pin.ToBytes(writer);
            }

            foreach (var pin in outputs)
            {
                pin.ToBytes(writer);
            }
        }

        protected virtual void OnCompose(Context ctx, CairoFont font)
        {
            cachedRenderX = Bounds.drawX;
            cachedRenderY = Bounds.drawY;

            nodeTransform.TransformPoint(ref cachedRenderX, ref cachedRenderY);

            var x = cachedRenderX;
            var y = cachedRenderY;

            var colX = Bounds.drawX;
            var colY = Bounds.drawY;

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

            var startColX = colX + (Constants.NODE_SCIPRT_DRAW_PADDING / 2.0);
            var startColY = colY + titleExtents.Height + (Constants.NODE_SCIPRT_DRAW_PADDING / 2.0);
            var startDrawX = x + (Constants.NODE_SCIPRT_DRAW_PADDING / 2.0);
            var startDrawY = y + titleExtents.Height + (Constants.NODE_SCIPRT_DRAW_PADDING / 2.0);
            var bigestWidth = 0.0d;
            var bigestHeight = 0.0d;

            x = startDrawX;
            y = startDrawY;

            colX = startColX;
            colY = startColY;

            foreach (var input in inputs)
            {
                input.Compose(colX, colY, x, y, ctx, font);
                y += input.Extents.Height + Constants.NODE_SCIPRT_TEXT_PADDING * Scale;
                colY += input.Extents.Height + Constants.NODE_SCIPRT_TEXT_PADDING * Scale;
                bigestWidth = bigestWidth > input.Extents.Width ? bigestWidth : input.Extents.Width;
                bigestHeight = bigestHeight > (y - startDrawY) ? bigestHeight : (y - startDrawY);
            }

            x += bigestWidth + Constants.NODE_SCIPRT_TEXT_PADDING * Scale;
            y = startDrawY;

            colX += bigestWidth + Constants.NODE_SCIPRT_TEXT_PADDING * Scale;
            colY = startColY;

            foreach (var output in outputs)
            {
                output.Compose(colX, colY, x, y, ctx, font);
                y += output.Extents.Height + Constants.NODE_SCIPRT_TEXT_PADDING * Scale;
                colY += output.Extents.Height + Constants.NODE_SCIPRT_TEXT_PADDING * Scale;
                bigestWidth = bigestWidth > (x + output.Extents.Width) - startDrawX ? bigestWidth : (x + output.Extents.Width) - startDrawX;
                bigestHeight = bigestHeight > (y - startDrawY) ? bigestHeight : (y - startDrawY);
            }

            bigestWidth = Math.Max(titleExtents.Width, bigestWidth);

            Bounds = Bounds.WithFixedSize(bigestWidth + (Constants.NODE_SCIPRT_DRAW_PADDING / 2.0), bigestHeight + titleExtents.Height + (Constants.NODE_SCIPRT_DRAW_PADDING / 2.0));
            Bounds.CalcWorldBounds();

            // re-compose to right align outputs.
            // this could probably be done more efficiently. 
            
            var farX = cachedRenderX + Bounds.InnerWidth;
            var farColX = Bounds.drawX + Bounds.InnerWidth;

            y = startDrawY;
            colY = startColY;

            foreach (var output in outputs)
            {
                output.Compose(farColX - output.Extents.Width - 4, colY, farX - output.Extents.Width - 4, y, ctx, font);
                y += output.Extents.Height + Constants.NODE_SCIPRT_TEXT_PADDING * Scale;
                colY += output.Extents.Height + Constants.NODE_SCIPRT_TEXT_PADDING * Scale;
            }

            ctx.Restore();

            isDirty = false;
        }

        public void MarkDirty()
        {
            isDirty = true;
        }

        public virtual bool MouseDown(double origx, double origy, double x, double y, EnumMouseButton button)
        {
            if (IsPositionInside((int)x, (int)y))
            {
                if (button == EnumMouseButton.Left || button == EnumMouseButton.Right)
                {
                    OnFocusGained();
                }

                foreach (var input in inputs)
                {
                    if (input.OnMouseDown(api, x, y, button))
                    {
                        if (button == EnumMouseButton.Middle)
                        {
                            var connection = input.TopConnection();
                            if (connection != null)
                            {
                                connection.DisconnectAll();
                            }
                        }
                        else if (button == EnumMouseButton.Left)
                        {
                            activeConnection = input.CreateConnection();
                            if (activeConnection != null)
                            {
                                activeConnection.DrawPoint.X = origx - Bounds.ParentBounds.absX;
                                activeConnection.DrawPoint.Y = origy - Bounds.ParentBounds.absY;
                            }
                            else
                            {
                                activePin = input;
                                state = ScriptNodeState.PinSelected;
                            }
                        }

                        return true;
                    }
                }

                foreach (var output in outputs)
                {
                    if (output.OnMouseDown(api, x, y, button))
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
                            activeConnection = output.CreateConnection();
                            if (activeConnection != null)
                            {
                                activeConnection.DrawPoint.X = origx - Bounds.ParentBounds.absX;
                                activeConnection.DrawPoint.Y = origy - Bounds.ParentBounds.absY;
                            }
                            else
                            {
                                state = ScriptNodeState.PinSelected;
                            }
                        }

                        return true;
                    }
                }

                return true;
            }
            else
            {
                activePin?.OnMouseDown(api, x, y, button);
                OnFocusLost();
                state = ScriptNodeState.None;
            }


            return false;
        }

        public virtual bool MouseUp(double origx, double origy, double x, double y, EnumMouseButton button)
        {
            if (IsPositionInside((int)x, (int)y))
            {
                if (state == ScriptNodeState.Dragged)
                {
                    OnFocusLost();
                    state = ScriptNodeState.None;
                }

                else if (activePin != null && activePin.OnMouseUp(api, x, y, button))
                {
                    state = ScriptNodeState.PinSelected;
                }
                else
                {
                    state = ScriptNodeState.Selected;
                }

                return true;
            }

            if (activeConnection != null)
            {
                if (activeConnection.IsConnected == false)
                {
                    activeConnection.DisconnectAll();
                }
                activeConnection = null;
            }
            else if (activePin != null)
            {
                activePin.OnMouseUp(api, x, y, button);
            }

            OnFocusLost();
            state = ScriptNodeState.None;

            return false;
        }

        public void MouseMove(double origx, double origy, double x, double y, double deltaX, double deltaY)
        {
            if (hasFocus && activeConnection == null && api.Input.MouseButton.Left && (Math.Abs(deltaX) > 0 || Math.Abs(deltaY) > 0))
            {
                Bounds = Bounds.WithFixedOffset(deltaX, deltaY);
                Bounds.CalcWorldBounds();

                state = ScriptNodeState.Dragged;
                isDirty = true;
            }
            else if (activeConnection != null)
            {
                activeConnection.DrawPoint.X += deltaX;
                activeConnection.DrawPoint.Y += deltaY;
            }
            else if (activePin != null)
            {
                activePin.OnMouseMove(api, x, y, deltaX, deltaY);
            }
        }

        public override void OnKeyDown(ICoreClientAPI api, KeyEvent args)
        {
            base.OnKeyDown(api, args);

            if (activePin != null)
            {
                activePin.OnKeyDown(api, args);
            }
        }

        public override void OnKeyPress(ICoreClientAPI api, KeyEvent args)
        {
            base.OnKeyPress(api, args);

            if (activePin != null)
            {
                activePin.OnKeyPress(api, args);
            }
        }

        public override void OnFocusLost()
        {
            base.OnFocusLost();

            activePin = null;
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (var input in inputs)
            {
                input.Dispose();
            }

            foreach (var output in outputs)
            {
                output.Dispose();
            }

            inputs.Clear();
            outputs.Clear();
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
