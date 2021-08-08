namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using VSCCI.Data;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Pins;

    public enum ScriptNodeState
    {
        Focused,
        Dragged,
        Selected,
        PinSelected,
        None
    }

    public enum HoverState
    {
        NotHovered,
        PendingHover,
        Hovered
    }

    public abstract class ScriptNode : GuiElement
    {
        protected readonly List<ScriptNodeInput> inputs;
        protected readonly List<ScriptNodeOutput> outputs;

        protected bool isDirty;

        protected ScriptNodeState state;

        protected string hoverText;

        private readonly TextDrawUtil textUtil;
        protected readonly CairoFont font;

        private ScriptNodePinBase activePin;

        private string title;
        private HoverState hoverState;
        private long hoverID;

        private object hoveredObject;

        private TextExtents titleExtents;

        private LoadedTexture staticTexture;
        private LoadedTexture selectedTexture;

        private HoverTextElement hoverTextElement;
        public bool IsDirty => isDirty;

        public Guid Guid;

        public bool IsSelected => state == ScriptNodeState.Selected;

        public ICoreClientAPI API => api;

        public ScriptNode(string _title, ICoreClientAPI api, MatrixElementBounds bounds) : base(api, bounds)
        {
            staticTexture = new LoadedTexture(api);
            selectedTexture = new LoadedTexture(api);

            bounds.IsDrawingSurface = true;

            textUtil = new TextDrawUtil();
            font = CairoFont.WhiteDetailText().WithFontSize(20);

            state = ScriptNodeState.None;

            hoveredObject = null;
            activePin = null;
            title = _title;

            inputs = new List<ScriptNodeInput>();
            outputs = new List<ScriptNodeOutput>();

            isDirty = true;
            hoverState = HoverState.NotHovered;
            titleExtents = new TextExtents();

            Guid = Guid.NewGuid();

            var b = MatrixElementBounds.Fixed(0, 0, 300, 150, null);
            bounds.ParentBounds.WithChild(b);
            b.CalcWorldBounds();
            hoverTextElement = new HoverTextElement(api, b);
        }

        public override void ComposeElements(Context ctxStatic, ImageSurface surface)
        {
            // calculates size
            ComposeSizeAndOffsets(ctxStatic, font);

            ComposeStaticElements();
            ComposeSelectedTexture();
        }

        protected virtual void ComposeStaticElements()
        {
            ImageSurface surface = new ImageSurface(Format.ARGB32, Bounds.OuterWidthInt, Bounds.OuterHeightInt); ;
            Context ctx = genContext(surface);

            var x = 0;
            var y = 0;

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
                input.RenderBackground(ctx, surface);
            }

            foreach (var output in outputs)
            {
                output.RenderBackground(ctx, surface);
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


            generateTexture(surface, ref staticTexture);

            surface.Dispose();
            ctx.Dispose();
        }

        void ComposeSelectedTexture()
        {
            ImageSurface surface = new ImageSurface(Format.ARGB32, Bounds.OuterWidthInt, Bounds.OuterHeightInt); ;
            Context ctx = genContext(surface);

            ctx.SetSourceRGBA(1.0, 1.0, 1.0, 0.3);
            RoundRectangle(ctx, 0, 0, Bounds.OuterWidth, Bounds.OuterHeight, 1);
            ctx.Fill();

            generateTexture(surface, ref selectedTexture);

            surface.Dispose();
            ctx.Dispose();
        }

        public override void RenderInteractiveElements(float deltaTime)
        {
            base.RenderInteractiveElements(deltaTime);

            if(isDirty)
            {
                ImageSurface surface = new ImageSurface(Format.ARGB32 ,100, 100);
                Context ctx = genContext(surface);

                ComposeElements(ctx, surface);

                ctx.Dispose();
                surface.Dispose();
            }

            api.Render.Render2DTexturePremultipliedAlpha(staticTexture.TextureId, Bounds, Constants.SCRIPT_NODE_Z_POS);

            foreach (var input in inputs)
            {
                input.RenderInteractive(deltaTime);
            }

            foreach (var output in outputs)
            {
                output.RenderInteractive(deltaTime);
            }

            if (state == ScriptNodeState.Selected)
            {
                api.Render.Render2DTexture(selectedTexture.TextureId, Bounds, Constants.SCRIPT_NODE_SELECTED_Z_POS);
            }

            if (hoverState == HoverState.Hovered)
            {
                hoverTextElement.RenderInteractiveElements(deltaTime);
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

        protected virtual void ComposeSizeAndOffsets(Context ctx, CairoFont font)
        {
            var x = 0.0;
            var y = 0.0;

            hoverTextElement.SetHoverText(GetNodeDescription());

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
            var bigestWidth = 0.0d;
            var bigestHeight = 0.0d;

            x = startDrawX;
            y = startDrawY;

            foreach (var input in inputs)
            {
                input.SetupSizeAndOffsets(x, y, ctx, font);
                y += input.Extents.Height + Constants.NODE_SCIPRT_TEXT_PADDING;
                bigestWidth = bigestWidth > input.Extents.Width ? bigestWidth : input.Extents.Width;
                bigestHeight = bigestHeight > (y - startDrawY) ? bigestHeight : (y - startDrawY);
            }

            x += bigestWidth + Constants.NODE_SCIPRT_TEXT_PADDING;
            y = startDrawY;

            foreach (var output in outputs)
            {
                output.SetupSizeAndOffsets(x, y, ctx, font);
                y += output.Extents.Height + Constants.NODE_SCIPRT_TEXT_PADDING;
                bigestWidth = bigestWidth > (x + output.Extents.Width) - startDrawX ? bigestWidth : (x + output.Extents.Width) - startDrawX;
                bigestHeight = bigestHeight > (y - startDrawY) ? bigestHeight : (y - startDrawY);
            }

            bigestWidth = Math.Max(titleExtents.Width, bigestWidth);

            Bounds = Bounds.WithFixedSize(bigestWidth + (Constants.NODE_SCIPRT_DRAW_PADDING / 2.0), bigestHeight + titleExtents.Height + (Constants.NODE_SCIPRT_DRAW_PADDING / 2.0));
            Bounds.CalcWorldBounds();

            // re-compose to right align outputs.
            // this could probably be done more efficiently. 
            
            var farX = Bounds.InnerWidth;

            y = startDrawY;

            foreach (var output in outputs)
            {
                output.SetupSizeAndOffsets(farX - output.Extents.Width - 4, y, ctx, font);
                y += output.Extents.Height + Constants.NODE_SCIPRT_TEXT_PADDING;
            }

            ctx.Restore();

            isDirty = false;
        }

        public void MarkDirty()
        {
            isDirty = true;
        }

        public override void OnMouseDown(ICoreClientAPI api, MouseEvent mouse)
        {
            base.OnMouseDown(api, mouse);

            var x = mouse.X;
            var y = mouse.Y;
            var button = mouse.Button;
            var previousHandled = mouse.Handled;
            mouse.Handled = false;

            if (IsPositionInside((int)x, (int)y))
            {
                if (button == EnumMouseButton.Left || button == EnumMouseButton.Right)
                {
                    OnFocusGained();
                    state = ScriptNodeState.Selected;
                }

                mouse.Handled = true;
            }

            foreach (var input in inputs)
            {
                if (input.OnMouseDown(api, mouse))
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
                        var activeConnection = input.CreateConnection();
                        if (activeConnection != null)
                        {
                            mouse.Handled = true;
                        }
                        else
                        {
                            activePin = input;
                            state = ScriptNodeState.PinSelected;
                        }
                    }
                }
            }

            foreach (var output in outputs)
            {
                if (output.OnMouseDown(api, mouse))
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
                        var activeConnection = output.CreateConnection();
                        if (activeConnection != null)
                        {
                            mouse.Handled = true;
                        }
                        else
                        {
                            state = ScriptNodeState.PinSelected;
                        }
                    }
                }
            }

            if(mouse.Handled == false && state != ScriptNodeState.None)
            {
                OnFocusLost();
                state = ScriptNodeState.None;
            }

            if(mouse.Handled == false)
            {
                mouse.Handled = previousHandled;
            }
        }

        public override void OnMouseUp(ICoreClientAPI api, MouseEvent mouse)
        {
            if (IsPositionInside(mouse.X, mouse.Y))
            {
                if (state == ScriptNodeState.Dragged)
                {
                    OnFocusLost();
                    state = ScriptNodeState.None;
                }

                else if (activePin != null && activePin.OnMouseUp(api, mouse))
                {
                    state = ScriptNodeState.PinSelected;
                }
                else
                {
                    state = ScriptNodeState.Selected;
                }

                mouse.Handled = true;
            }

            foreach (var input in inputs)
            {
                input.OnMouseUp(api, mouse);
            }

            foreach (var output in outputs)
            {
                output.OnMouseUp(api, mouse);
            }

            if (mouse.Handled == false && state != ScriptNodeState.None)
            {
                OnFocusLost();
                state = ScriptNodeState.None;
            }
        }

        public override void OnMouseMove(ICoreClientAPI api, MouseEvent mouse)
        {
            var x = mouse.X;
            var y = mouse.Y;

            var deltaX = mouse.DeltaX;
            var deltaY = mouse.DeltaY;

            if (hasFocus && api.Input.MouseButton.Left && (Math.Abs(deltaX) > 0 || Math.Abs(deltaY) > 0))
            {
                Bounds = Bounds.WithFixedOffset(deltaX, deltaY);
                Bounds.CalcWorldBounds();

                state = ScriptNodeState.Dragged;
                isDirty = true;

                mouse.Handled = true;

                if (hoverState == HoverState.PendingHover) api.Event.UnregisterCallback(hoverID);
                hoverState = HoverState.NotHovered;
            }
            else if (api.Input.MouseButton.Left)
            {
                if (hoverState == HoverState.PendingHover) api.Event.UnregisterCallback(hoverID);
                hoverState = HoverState.NotHovered;
            }
            else if (activePin != null)
            {
                activePin.OnMouseMove(api, mouse);

                if (hoverState == HoverState.PendingHover) api.Event.UnregisterCallback(hoverID);
                hoverState = HoverState.NotHovered;
            }
            else // nothing else is happening so we prepare for hover text
            {
                if (IsPositionInside(x, y))
                {
                    if (hoverState == HoverState.NotHovered)
                    {
                        hoverState = HoverState.PendingHover;
                        hoverID = api.Event.RegisterCallback(SetHovered, Constants.HOVER_DELAY);
                        hoverTextElement.SetPosition((x + Constants.HOVER_DISPLAY_X_OFFSET) - Bounds.ParentBounds.absX, (y + Constants.HOVER_DISPLAY_Y_OFFSET) - Bounds.ParentBounds.absY);
                        DetermineCorrectHoverText(x, y);
                    }
                    else if(hoverState == HoverState.Hovered)
                    {
                        hoverTextElement.SetPosition((x + Constants.HOVER_DISPLAY_X_OFFSET) - Bounds.ParentBounds.absX, (y + Constants.HOVER_DISPLAY_Y_OFFSET) - Bounds.ParentBounds.absY);

                        DetermineCorrectHoverText(x, y);
                    }
                }
                else
                {
                    if (hoverState == HoverState.PendingHover) api.Event.UnregisterCallback(hoverID);
                    hoverState = HoverState.NotHovered;
                }
            }
        }

        private void DetermineCorrectHoverText(int x, int y)
        {
            foreach (var input in inputs)
            {
                if (input.PointIsWithinHoverBounds(x, y))
                {
                    if(hoveredObject == input)
                    {
                        return;
                    }

                    hoveredObject = input;
                    hoverTextElement.SetHoverText(input.GetHoverText());
                    return;
                }
            }

            foreach (var output in outputs)
            {
                if (output.PointIsWithinHoverBounds(x, y))
                {
                    if (hoveredObject == output)
                    {
                        return;
                    }

                    hoveredObject = output;
                    hoverTextElement.SetHoverText(output.GetHoverText());
                    return;
                }
            }

            if (hoveredObject == this)
            {
                return;
            }

            hoveredObject = this;
            hoverTextElement.SetHoverText(GetNodeDescription());
        }

        private void SetHovered(float _)
        {
            hoverState = HoverState.Hovered;
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

            staticTexture.Dispose();
            selectedTexture.Dispose();
        }

        public bool ConnectionWillConnectToPoint(ScriptNodePinConnection connection, double x, double y)
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

        public abstract string GetNodeDescription();
    }
}
