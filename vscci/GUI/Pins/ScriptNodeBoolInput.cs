namespace VSCCI.GUI.Pins
{
    using Cairo;
    using System;
    using System.IO;
    using Vintagestory.API.Client;
    using VSCCI.Data;
    using VSCCI.GUI.Nodes;

    public class ScriptNodeBoolInput : ScriptNodeInput
    {
        GuiElementToggleButton buttonInput;
        Action<bool> buttonPressed;

        public ScriptNodeBoolInput(ScriptNode owner, string name = "") : base(owner, name, typeof(bool))
        {
            buttonInput = new GuiElementToggleButton(api, "", "Is True", CairoFont.WhiteSmallText().WithFontSize(15), OnButtonToggleChanged, ElementBounds.Empty, true);
        }

        public ScriptNodeBoolInput(ScriptNode owner, Action<bool> buttonPressed, string name = "") : base(owner, name, typeof(bool))
        {
            buttonInput = new GuiElementToggleButton(api, "", "Is True", CairoFont.WhiteSmallText().WithFontSize(15), OnButtonToggleChanged, ElementBounds.Empty, true);
            // default to true
            isDirty = false;
            this.buttonPressed = buttonPressed;
        }

        public override void RenderBackground(Context ctx, ImageSurface surface)
        {
            buttonInput.ComposeElements(ctx, surface);
        }

        public override void RenderText(TextDrawUtil textUtil, CairoFont font, Context ctx, ImageSurface surface)
        {
            ctx.SetSourceRGBA(1, 1, 1, 1.0);
            ctx.Save();
            ctx.MoveTo(X + buttonInput.Bounds.fixedWidth + Constants.NODE_SCIPRT_TEXT_PADDING, (Y + (pinExtents.Height / 2.0)) + (textExtents.Height / 2.0));
            ctx.ShowText(name);
            ctx.Restore();
        }

        public override void RenderInteractive(float deltaTime)
        {
            if (hasConnection == false)
            {
                buttonInput.RenderInteractiveElements(deltaTime);
            }
        }

        public override void RenderPin(Context ctx, ImageSurface surface)
        {
            if(hasConnection)
            {
                base.RenderPin(ctx, surface);
            }
        }

        public override void OnPinConneced(ScriptNodePinConnection connection)
        {
            base.OnPinConneced(connection);
            isDirty = true;
        }

        public override void OnPinDisconnected(ScriptNodePinConnection connection)
        {
            base.OnPinDisconnected(connection);
            isDirty = true;
        }

        public override void SetupSizeAndOffsets(double x, double y, Context ctx, CairoFont font)
        {
            X = x;
            Y = y;

            textExtents = ctx.TextExtents(name);
            
            owner.Bounds.ChildBounds.Remove(buttonInput.Bounds);
            buttonInput.Bounds = ElementBounds.Fixed(X, Y + 4, 60, 30);
            owner.Bounds.WithChild(buttonInput.Bounds);
            buttonInput.Bounds.CalcWorldBounds();

            pinExtents.Width = buttonInput.Bounds.OuterWidth + textExtents.Width + Constants.NODE_SCIPRT_TEXT_PADDING;
            pinExtents.Height = buttonInput.Bounds.OuterHeight;

            isDirty = true;

            if (pinSelectBounds != null)
            {
                owner.Bounds.ChildBounds.Remove(pinSelectBounds);
            }

            pinSelectBounds = ElementBounds.Fixed(X, Y, buttonInput.Bounds.OuterWidth, buttonInput.Bounds.OuterHeight);
            owner.Bounds.WithChild(pinSelectBounds);
            pinSelectBounds.CalcWorldBounds();

            if (hoverBounds != null)
            {
                owner.Bounds.ChildBounds.Remove(hoverBounds);
            }

            hoverBounds = ElementBounds.Fixed(x, y, buttonInput.Bounds.OuterWidth + textExtents.Width + Constants.NODE_SCIPRT_TEXT_PADDING, buttonInput.Bounds.OuterHeight);
            owner.Bounds.WithChild(hoverBounds);
            hoverBounds.CalcWorldBounds();

            pinConnectionPoint.X = owner.Bounds.drawX + X + (buttonInput.Bounds.OuterWidth / 2.0);
            pinConnectionPoint.Y = owner.Bounds.drawY + Y + (buttonInput.Bounds.OuterHeight / 2.0);

            //textInput.Font = font;
        }

        public override ScriptNodePinConnection CreateConnection()
        {
            // we never create a connection from clicking on this but we can drop a connection onto it to connect
            return null;
        }

        public override dynamic GetInput()
        {
            if(hasConnection)
            {
                return base.GetInput();
            }

            return buttonInput.On;
        }

        public override void FromBytes(BinaryReader reader)
        {
            base.FromBytes(reader);

            buttonInput.SetValue(reader.ReadBoolean());
        }

        public override void ToBytes(BinaryWriter writer)
        {
            base.ToBytes(writer);

            writer.Write(buttonInput.On);
        }

        public override bool OnMouseDown(ICoreClientAPI api, MouseEvent mouse)
        {
            if (buttonInput.IsPositionInside(mouse.X, mouse.Y))
            {
                buttonInput.OnFocusGained();
                buttonInput.OnMouseDownOnElement(api, mouse);
                return true;
            }
            else
            {
                buttonInput.OnFocusLost();
                return false;
            }
        }

        public override void OnMouseMove(ICoreClientAPI api, MouseEvent mouse)
        {
            buttonInput.OnMouseMove(api, mouse);

        }
        public override bool OnMouseUp(ICoreClientAPI api, MouseEvent mouse)
        {
            buttonInput.OnMouseUp(api, mouse);
            if (buttonInput.IsPositionInside(mouse.X, mouse.Y) == false)
            {
                buttonInput.OnFocusLost();
                return false;
            }

            return true;
        }

        public override void Dispose()
        {
            base.Dispose();

            buttonInput.Dispose(); 
        }

        private void OnButtonToggleChanged(bool toggled)
        {
            buttonPressed?.Invoke(toggled);
        }
    }
}
