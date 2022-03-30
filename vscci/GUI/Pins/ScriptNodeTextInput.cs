using System;

namespace VSCCI.GUI.Pins
{
    using Cairo;
    using System.IO;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using VSCCI.GUI.Nodes;

    public class ScriptNodeTextInput : ScriptNodeInput
    {
        protected GuiElementTextInput textInput;

        public Func<char, bool> IsKeyAllowed;
        public event Action<string> TextChanged;

        bool textInputNeedsCompose;

        public ScriptNodeTextInput(ScriptNode owner, System.Type pinType) : base(owner, "", pinType)
        {
            textInput = new GuiElementTextInput(api, ElementBounds.Empty, OnTextChanged, CairoFont.WhiteSmallText().WithFontSize(20));
            // default to true
            IsKeyAllowed = (char c) => { return true; };
            textInputNeedsCompose = false;
            isDirty = false;
        }

        public override void RenderBackground(Context ctx, ImageSurface surface)
        {
            if (textInputNeedsCompose)
            {
                textInput.ComposeElements(ctx, surface);
                textInputNeedsCompose = false;
                isDirty = false;
            }
        }

        public override void RenderText(TextDrawUtil textUtil, CairoFont font, Context ctx, ImageSurface surface)
        {
        }

        public override void RenderInteractive(float deltaTime)
        {
            if (hasConnection == false && textInputNeedsCompose == false)
            {
                textInput.RenderInteractiveElements(deltaTime);
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

            textInputNeedsCompose = true;
        }

        public override void OnPinDisconnected(ScriptNodePinConnection connection)
        {
            base.OnPinDisconnected(connection);

            textInput.Dispose();
        }

        public override void SetupSizeAndOffsets(double x, double y, Context ctx, CairoFont font)
        {
            X = x;
            Y = y;

            owner.Bounds.ChildBounds.Remove(textInput.Bounds);
            textInput.Bounds = ElementBounds.Fixed(X, Y + 4, 100, 30);
            owner.Bounds.WithChild(textInput.Bounds);
            textInput.Bounds.CalcWorldBounds();

            pinExtents.Width = textInput.Bounds.OuterWidth;
            pinExtents.Height = textInput.Bounds.OuterHeight;

            isDirty = true;
            textInputNeedsCompose = true;

            if (pinSelectBounds != null)
            {
                owner.Bounds.ChildBounds.Remove(pinSelectBounds);
            }

            pinSelectBounds = ElementBounds.Fixed(x, y, textInput.Bounds.OuterWidth, textInput.Bounds.OuterHeight);
            owner.Bounds.WithChild(pinSelectBounds);
            pinSelectBounds.CalcWorldBounds();

            if (hoverBounds != null)
            {
                owner.Bounds.ChildBounds.Remove(hoverBounds);
            }

            hoverBounds = ElementBounds.Fixed(x, y, textInput.Bounds.OuterWidth, textInput.Bounds.OuterHeight);
            owner.Bounds.WithChild(hoverBounds);
            hoverBounds.CalcWorldBounds();

            pinConnectionPoint.X = owner.Bounds.drawX + X + (textInput.Bounds.OuterWidth / 2.0);
            pinConnectionPoint.Y = owner.Bounds.drawY + Y + (textInput.Bounds.OuterHeight / 2.0);

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

            return textInput.GetText();
        }

        public override void FromBytes(BinaryReader reader)
        {
            base.FromBytes(reader);

            textInput.SetValue(reader.ReadString());
            textInput.SetCaretPos(0);
        }

        public override void ToBytes(BinaryWriter writer)
        {
            base.ToBytes(writer);

            writer.Write(textInput.GetText());
        }

        public void ClearText()
        {
            SetText("");
        }

        public void SetText(string text)
        {
            textInput.SetValue(text);
        }

        public string GetText()
        {
            return textInput.GetText();
        }

        public override void OnKeyDown(ICoreClientAPI api, KeyEvent args)
        {
            textInput.OnKeyDown(api, args);
        }

        public override void OnKeyPress(ICoreClientAPI api, KeyEvent args)
        {
            if (IsKeyAllowed(args.KeyChar))
            {
                textInput.OnKeyPress(api, args);
            }
        }

        public override bool OnMouseDown(ICoreClientAPI api, MouseEvent mouse)
        {
            if (textInput.IsPositionInside(mouse.X, mouse.Y))
            {
                textInput.OnFocusGained();
                textInput.OnMouseDownOnElement(api, mouse);
                return true;
            }
            else
            {
                textInput.OnFocusLost();
                return false;
            }
        }

        public override void OnMouseMove(ICoreClientAPI api, MouseEvent mouse)
        {
            textInput.OnMouseMove(api, mouse);

        }
        public override bool OnMouseUp(ICoreClientAPI api, MouseEvent mouse)
        {
            textInput.OnMouseUp(api, mouse);
            if (PointIsWithinSelectionBounds(mouse.X, mouse.Y) == false)
            {
                textInput.OnFocusLost();
                return false;
            }

            return true;
        }

        public override void Dispose()
        {
            base.Dispose();

            textInput.Dispose(); 
        }
        private void OnTextChanged(string text)
        {
            TextChanged?.Invoke(text);
        }
    }
}
