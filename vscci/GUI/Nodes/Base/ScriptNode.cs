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

        protected ScriptNodeState currentState;
        protected ScriptNodeState previousState;

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

        public Guid Guid { get; private set; }

        public event EventHandler<bool> onSelectedChanged;

        public bool IsSelected => currentState == ScriptNodeState.Selected;

        public ICoreClientAPI API => api;

        public ScriptNode(string _title, ICoreClientAPI api, MatrixElementBounds bounds) : base(api, bounds)
        {
            staticTexture = new LoadedTexture(api);
            selectedTexture = new LoadedTexture(api);

            bounds.IsDrawingSurface = true;

            textUtil = new TextDrawUtil();
            font = CairoFont.WhiteDetailText().WithFontSize(20);

            currentState = ScriptNodeState.None;
            previousState = ScriptNodeState.None;

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

        private void ComposeSelectedTexture()
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
                isDirty = false;
            }

            api.Render.PushScissor(Bounds.ParentBounds, true);

            api.Render.Render2DTexturePremultipliedAlpha(staticTexture.TextureId, Bounds, Constants.SCRIPT_NODE_Z_POS);

            foreach (var input in inputs)
            {
                input.RenderInteractive(deltaTime);
            }

            foreach (var output in outputs)
            {
                output.RenderInteractive(deltaTime);
            }

            if (currentState == ScriptNodeState.Selected)
            {
                api.Render.Render2DTexture(selectedTexture.TextureId, Bounds, Constants.SCRIPT_NODE_SELECTED_Z_POS);
            }

            if (hoverState == HoverState.Hovered)
            {
                hoverTextElement.RenderInteractiveElements(deltaTime);
            }

            api.Render.PopScissor();
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

        public virtual void ReadPinsFromBytes(BinaryReader reader)
        {
            foreach (var pin in inputs)
            {
                pin.FromBytes(reader);
            }

            foreach (var pin in outputs)
            {
                pin.FromBytes(reader);
            }

            MarkDirty();
        }

        public virtual void WrtiePinsToBytes(BinaryWriter writer)
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

        public void RegeneratedGUIDs()
        {
            SetGuid(Guid.NewGuid());

            foreach(var input in inputs)
            {
                input.Guid = Guid.NewGuid();
            }

            foreach (var output in outputs)
            {
                output.Guid = Guid.NewGuid();
            }
        }

        public virtual void SetGuid(Guid newGuid)
        {
            Guid = newGuid;
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
            var startDrawY = y + titleExtents.Height + Constants.NODE_SCIPRT_DRAW_PADDING;
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

            Bounds = Bounds.WithFixedSize(bigestWidth + (Constants.NODE_SCIPRT_DRAW_PADDING / 2.0), bigestHeight + titleExtents.Height + Constants.NODE_SCIPRT_DRAW_PADDING);
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
        }

        public void MarkDirty()
        {
            isDirty = true;

            foreach(var input in inputs)
            {
                input.MarkDirty();
            }

            foreach (var output in outputs)
            {
                output.MarkDirty();
            }

            ScriptNodePinConnectionManager.TheManage.MarkDirty();
        }

        public virtual void OnMouseDown(ICoreClientAPI api, NodeMouseEvent @event)
        {
            var mouse = @event.mouseEvent;

            base.OnMouseDown(api, mouse);

            var x = mouse.X;
            var y = mouse.Y;
            var button = mouse.Button;
            var previousState = currentState;
            var previousHandled = mouse.Handled;
            mouse.Handled = false;

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
                            SetState(ScriptNodeState.PinSelected);
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
                            SetState(ScriptNodeState.PinSelected);
                        }
                    }
                }
            }

            if (@event.intersectingNode == this && mouse.Handled == false)
            {
                if (button == EnumMouseButton.Left || button == EnumMouseButton.Right)
                {
                    OnFocusGained();
                }

                mouse.Handled = true;
            }
            else if(mouse.Handled == false && currentState != ScriptNodeState.None && CntrlPressed() == false && @event.intersectingNode == null)
            {
                if ((currentState == ScriptNodeState.Selected && previousHandled) || currentState != ScriptNodeState.Selected)
                {
                    OnFocusLost();
                    SetState(ScriptNodeState.None);
                }
            }

            if(mouse.Handled == false)
            {
                mouse.Handled = previousHandled;
            }
        }

        public virtual void OnMouseUp(ICoreClientAPI api, NodeMouseEvent @event)
        {
            var mouse = @event.mouseEvent;

            var previousState = currentState;
            var pr = mouse.Handled;
            mouse.Handled = false;

            foreach (var input in inputs)
            {
                mouse.Handled |= input.OnMouseUp(api, mouse);
            }

            foreach (var output in outputs)
            {
                mouse.Handled |= output.OnMouseUp(api, mouse);
            }

            if (mouse.Handled == false)
            {
                if (@event.intersectingNode == this)
                {
                    if (currentState == ScriptNodeState.Dragged && @event.nodeSelectCount <= 1)
                    {
                        OnFocusLost();
                        SetState(ScriptNodeState.None);
                    }
                    else if (currentState != ScriptNodeState.Dragged)
                    {
                        SetState(ScriptNodeState.Selected);
                    }

                    mouse.Handled = true;
                }
            }

            if (mouse.Handled == false && currentState != ScriptNodeState.None && CntrlPressed() == false)
            {
                OnFocusLost();
                SetState(ScriptNodeState.None);
            }

            if(mouse.Handled == false)
            {
                mouse.Handled = pr;
            }
        }

        public override void OnMouseMove(ICoreClientAPI api, MouseEvent mouse)
        {
            var x = mouse.X;
            var y = mouse.Y;

            var deltaX = mouse.DeltaX;
            var deltaY = mouse.DeltaY;

            if ((hasFocus || currentState == ScriptNodeState.Selected || currentState == ScriptNodeState.Dragged) && api.Input.MouseButton.Left && (Math.Abs(deltaX) > 0 || Math.Abs(deltaY) > 0))
            {
                Bounds = Bounds.WithFixedOffset(deltaX, deltaY);
                Bounds.CalcWorldBounds();

                SetState(ScriptNodeState.Dragged);
                MarkDirty();

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

            staticTexture.Dispose();
            selectedTexture.Dispose();
        }

        public void RemoveAllConnections()
        {
            foreach (var input in inputs)
            {
                input.ClearConnections();
            }

            foreach (var output in outputs)
            {
                output.ClearConnections();
            }

            inputs.Clear();
            outputs.Clear();
        }

        public void FallbackState()
        {
            SetState(previousState);
        }

        public void SetState(ScriptNodeState state)
        {
            if (currentState != state)
            {
                if (currentState == ScriptNodeState.Selected && state != ScriptNodeState.Dragged)
                {
                    onSelectedChanged?.Invoke(this, false);
                }
                else if (state == ScriptNodeState.Selected)
                {
                    onSelectedChanged?.Invoke(this, true);
                }
                else if(previousState == ScriptNodeState.Selected && currentState == ScriptNodeState.Dragged)
                {
                    onSelectedChanged?.Invoke(this, false);
                }

                previousState = currentState;
                currentState = state;
            }
        }

        protected bool CntrlPressed()
        {
            return  api.Input.KeyboardKeyStateRaw[(int)GlKeys.LControl] ||  api.Input.KeyboardKeyStateRaw[(int)GlKeys.RControl];
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
