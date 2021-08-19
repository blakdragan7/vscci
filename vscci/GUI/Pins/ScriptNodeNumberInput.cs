namespace VSCCI.GUI.Pins
{
    using Cairo;
    using System;
    using System.IO;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using VSCCI.Data;
    using VSCCI.GUI.Nodes;

    public class ScriptNodeNumberInput : ScriptNodeInput
    {
        protected GuiElementNumberInput numberInput;

        public ScriptNodeNumberInput(ScriptNode owner, string name) : base(owner, name, typeof(Number))
        {
            numberInput = new GuiElementNumberInput(api, ElementBounds.Empty, OnTextChanged, CairoFont.WhiteSmallText().WithFontSize(20));
            // default to true
            isDirty = false;
        }

        public override void RenderBackground(Context ctx, ImageSurface surface)
        {
            numberInput.ComposeElements(ctx, surface);
        }

        public override void RenderText(TextDrawUtil textUtil, CairoFont font, Context ctx, ImageSurface surface)
        {
            ctx.SetSourceRGBA(1, 1, 1, 1.0);
            ctx.Save();
            ctx.MoveTo(X + numberInput.Bounds.fixedWidth + Constants.NODE_SCIPRT_TEXT_PADDING, (Y + (pinExtents.Height / 2.0)) + (textExtents.Height / 2.0));
            ctx.ShowText(name);
            ctx.Restore();
        }

        public override void RenderInteractive(float deltaTime)
        {
            if (hasConnection == false)
            {
                numberInput.RenderInteractiveElements(deltaTime);
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

            MarkDirty();
        }

        public override void OnPinDisconnected(ScriptNodePinConnection connection)
        {
            base.OnPinDisconnected(connection);

            MarkDirty();
        }

        public override void SetupSizeAndOffsets(double x, double y, Context ctx, CairoFont font)
        {
            X = x;
            Y = y;

            textExtents = ctx.TextExtents(name);

            owner.Bounds.ChildBounds.Remove(numberInput.Bounds);
            numberInput.Bounds = ElementBounds.Fixed(X, Y + 4, 100, 30);
            owner.Bounds.WithChild(numberInput.Bounds);
            numberInput.Bounds.CalcWorldBounds();

            pinExtents.Width = numberInput.Bounds.OuterWidth + textExtents.Width + Constants.NODE_SCIPRT_TEXT_PADDING;
            pinExtents.Height = Math.Max(numberInput.Bounds.OuterHeight, textExtents.Height);

            isDirty = true;

            if (pinSelectBounds != null)
            {
                owner.Bounds.ChildBounds.Remove(pinSelectBounds);
            }

            pinSelectBounds = ElementBounds.Fixed(x, y, numberInput.Bounds.OuterWidth, numberInput.Bounds.OuterHeight);
            owner.Bounds.WithChild(pinSelectBounds);
            pinSelectBounds.CalcWorldBounds();

            if (hoverBounds != null)
            {
                owner.Bounds.ChildBounds.Remove(hoverBounds);
            }

            hoverBounds = ElementBounds.Fixed(x, y, numberInput.Bounds.OuterWidth, numberInput.Bounds.OuterHeight);
            owner.Bounds.WithChild(hoverBounds);
            hoverBounds.CalcWorldBounds();

            pinConnectionPoint.X = owner.Bounds.drawX + X + (numberInput.Bounds.OuterWidth / 2.0);
            pinConnectionPoint.Y = owner.Bounds.drawY + Y + (numberInput.Bounds.OuterHeight / 2.0);
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

            return Number.Parse(numberInput.GetText());
        }

        public override void FromBytes(BinaryReader reader)
        {
            base.FromBytes(reader);

            numberInput.SetValue(reader.ReadString());
            numberInput.SetCaretPos(0);
        }

        public override void ToBytes(BinaryWriter writer)
        {
            base.ToBytes(writer);

            writer.Write(numberInput.GetText());
        }

        public void ClearText()
        {
            SetText("");
        }

        public void SetText(string text)
        {
            numberInput.SetValue(text);
        }

        public string GetText()
        {
            return numberInput.GetText();
        }

        public override void OnKeyDown(ICoreClientAPI api, KeyEvent args)
        {
            numberInput.OnKeyDown(api, args);
        }

        public override void OnKeyPress(ICoreClientAPI api, KeyEvent args)
        {
            numberInput.OnKeyPress(api, args);
        }

        public override bool OnMouseDown(ICoreClientAPI api, MouseEvent mouse)
        {
            if (numberInput.IsPositionInside(mouse.X, mouse.Y))
            {
                numberInput.OnFocusGained();
                numberInput.OnMouseDownOnElement(api, mouse);
                return true;
            }
            else
            {
                numberInput.OnFocusLost();
                return false;
            }
        }

        public override void OnMouseMove(ICoreClientAPI api, MouseEvent mouse)
        {
            numberInput.OnMouseMove(api, mouse);

        }
        public override bool OnMouseUp(ICoreClientAPI api, MouseEvent mouse)
        {
            numberInput.OnMouseUp(api, mouse);
            if (numberInput.IsPositionInside(mouse.X, mouse.Y) == false)
            {
                numberInput.OnFocusLost();
                return false;
            }

            return true;
        }

        public override void Dispose()
        {
            base.Dispose();

            numberInput.Dispose(); 
        }
        private void OnTextChanged(string text)
        {
        }
    }
}
