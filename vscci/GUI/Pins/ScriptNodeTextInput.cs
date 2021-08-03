namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using System.IO;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;

    public class ScriptNodeTextInput : ScriptNodeInput
    {
        protected GuiElementTextInput textInput;

        public Func<char, bool> IsKeyAllowed;
        public event Action<string> TextChanged;

        ICoreClientAPI api;

        bool textInputNeedsCompose;

        public ScriptNodeTextInput(ScriptNode owner, ICoreClientAPI api,  System.Type pinType) : base(owner, "", pinType)
        {
            textInput = new GuiElementTextInput(api, ElementBounds.Empty, OnTextChanged, CairoFont.WhiteSmallText().WithFontSize(20));
            // default to true
            IsKeyAllowed = (char c) => { return true; };
            this.api = api;
            textInputNeedsCompose = false;
        }

        public override void RenderOther(Context ctx, ImageSurface surface, double deltaTime)
        {
            if (hasConnection == false)
            {
                var bounds = textInput.Bounds;
                ctx.SetSourceRGBA(0.1, 0.1, 0.1, 0.5);
                RoundRectangle(ctx, bounds.drawX, bounds.drawY, bounds.InnerWidth, bounds.InnerHeight, 1.0);
                ctx.Fill();
            }
        }

        public override void RenderText(TextDrawUtil textUtil, CairoFont font, Context ctx, ImageSurface surface, double deltaTime)
        {
            if(textInputNeedsCompose)
            {
                textInput.ComposeElements(ctx, surface);
                textInputNeedsCompose = false;
            }
        }

        public override void RenderInteractive(double deltaTime)
        {
            if (hasConnection == false && textInputNeedsCompose == false)
            {
                textInput.Enabled = true;
                textInput.RenderInteractiveElements((float)deltaTime);
            }
            else
            {
                textInput.Enabled = false;
            }
        }

        public override void RenderPin(Context ctx, ImageSurface surface, double deltaTime)
        {
            if(hasConnection)
            {
                ctx.SetSourceColor(PinColor);
                ctx.LineWidth = 2;
                RoundRectangle(ctx, textInput.Bounds.drawX, textInput.Bounds.drawY, textInput.Bounds.InnerHeight, textInput.Bounds.InnerHeight, GuiStyle.ElementBGRadius);
                ctx.Fill();
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

        public override void Compose(double colx, double coly, double drawx, double drawy, Context ctx, CairoFont font)
        {
            X = drawx;
            Y = drawy;

            owner.Bounds.ParentBounds.ChildBounds.Remove(textInput.Bounds);
            textInput.Bounds = ElementBounds.Fixed(X, Y, 100, 30);
            owner.Bounds.ParentBounds.WithChild(textInput.Bounds);
            textInput.Bounds.CalcWorldBounds();

            extents.Width = textInput.Bounds.OuterWidth;
            extents.Height = textInput.Bounds.OuterHeight;

            isDirty = false;

            if (pinSelectBounds != null)
            {
                owner.Bounds.ParentBounds.ChildBounds.Remove(pinSelectBounds);
            }

            pinSelectBounds = ElementBounds.Fixed(colx, coly, extents.Width, extents.Height);
            owner.Bounds.ParentBounds.WithChild(pinSelectBounds);
            pinSelectBounds.CalcWorldBounds();

            if (hoverBounds != null)
            {
                owner.Bounds.ParentBounds.ChildBounds.Remove(hoverBounds);
            }

            hoverBounds = ElementBounds.Fixed(colx, coly, extents.Width, extents.Height);
            owner.Bounds.ParentBounds.WithChild(hoverBounds);
            hoverBounds.CalcWorldBounds();

            pinConnectionPoint.X = textInput.Bounds.drawX + (textInput.Bounds.InnerHeight / 2.0);
            pinConnectionPoint.Y = textInput.Bounds.drawY + (textInput.Bounds.InnerHeight / 2.0);

            //textInput.Font = font;
            textInput.ComposeElements(ctx, null);
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

        public override bool OnMouseDown(ICoreClientAPI api, double x, double y, EnumMouseButton button)
        {
            if (PointIsWithinSelectionBounds(x, y))
            {
                textInput.OnFocusGained();
                var mevent = new MouseEvent((int)x, (int)y, button);
                textInput.OnMouseDownOnElement(api, mevent);
                return true;
            }
            else
            {
                textInput.OnFocusLost();
                return false;
            }
        }

        public override void OnMouseMove(ICoreClientAPI api, double x, double y, double deltaX, double deltaY)
        {
            var mevent = new MouseEvent((int)x, (int)y, (int)deltaX, (int)deltaY);
            textInput.OnMouseMove(api, mevent);
        }
        public override bool OnMouseUp(ICoreClientAPI api, double x, double y, EnumMouseButton button)
        {
            var mevent = new MouseEvent((int)x, (int)y, button);
            textInput.OnMouseUp(api, mevent);
            if (PointIsWithinSelectionBounds(x, y) == false)
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
