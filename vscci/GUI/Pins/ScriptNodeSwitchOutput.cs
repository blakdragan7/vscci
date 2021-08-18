namespace VSCCI.GUI.Pins
{
    using Cairo;
    using System.IO;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using VSCCI.GUI.Nodes;

    public class ScriptNodeSwitchOutput : ExecOutputNode
    {
        protected GuiElementTextInput valueInput;
        protected GuiElementToggleButton cancelButton;

        public Func<char, bool> IsKeyAllowed;
        public event Action<string> TextChanged;

        Action<ScriptNodeSwitchOutput> OnRemoved;

        public ScriptNodeSwitchOutput(ExecutableScriptNode owner, Action<ScriptNodeSwitchOutput> OnRemoved, string title) : base(owner, title)
        {
            this.OnRemoved = OnRemoved;

            cancelButton = new GuiElementToggleButton(api, null, "-", CairoFont.ButtonText(), OnCancelClicked, ElementBounds.Empty);
            valueInput = new GuiElementTextInput(api, ElementBounds.Empty, OnTextChanged, CairoFont.WhiteSmallText().WithFontSize(20));
            
            // default to true
            IsKeyAllowed = (char c) => { return true; };
        }

        public override void RenderBackground(Context ctx, ImageSurface surface)
        {
            cancelButton.ComposeElements(ctx, surface);
            valueInput.ComposeElements(ctx, surface);
        }

        public override void RenderText(TextDrawUtil textUtil, CairoFont font, Context ctx, ImageSurface surface)
        {
        }

        public override void RenderInteractive(float deltaTime)
        {
            base.RenderInteractive(deltaTime);

            cancelButton.RenderInteractiveElements(deltaTime);
            valueInput.RenderInteractiveElements(deltaTime);
        }

        public override void SetupSizeAndOffsets(double x, double y, Context ctx, CairoFont font)
        {
            X = x;
            Y = y;

            owner.Bounds.ChildBounds.Remove(cancelButton.Bounds);
            cancelButton.Bounds = ElementBounds.Fixed(X, Y, 30, 30);
            owner.Bounds.WithChild(cancelButton.Bounds);
            cancelButton.Bounds.CalcWorldBounds();

            owner.Bounds.ChildBounds.Remove(valueInput.Bounds);
            valueInput.Bounds = ElementBounds.Fixed(cancelButton.Bounds.fixedX + 4 + cancelButton.Bounds.fixedWidth, Y, 100, 30);
            owner.Bounds.WithChild(valueInput.Bounds);
            valueInput.Bounds.CalcWorldBounds();

            pinExtents.Width = cancelButton.Bounds.fixedWidth + valueInput.Bounds.fixedWidth + DefaultPinSize + 4 + 4;
            pinExtents.Height = System.Math.Max(System.Math.Max(cancelButton.Bounds.OuterHeight, valueInput.Bounds.OuterHeight), DefaultPinSize);

            if (pinSelectBounds != null)
            {
                owner.Bounds.ChildBounds.Remove(pinSelectBounds);
            }

            pinSelectBounds = ElementBounds.Fixed(valueInput.Bounds.fixedX + valueInput.Bounds.fixedWidth + 4, y + (pinExtents.Height / 2.0) - (DefaultPinSize / 2.0), DefaultPinSize, DefaultPinSize);
            owner.Bounds.WithChild(pinSelectBounds);
            pinSelectBounds.CalcWorldBounds();

            if (hoverBounds != null)
            {
                owner.Bounds.ChildBounds.Remove(hoverBounds);
            }

            hoverBounds = ElementBounds.Fixed(x, y, pinExtents.Width, pinExtents.Height);
            owner.Bounds.WithChild(hoverBounds);
            hoverBounds.CalcWorldBounds();

            pinConnectionPoint.X = owner.Bounds.drawX + pinSelectBounds.drawX + (pinSelectBounds.OuterWidth / 2.0);
            pinConnectionPoint.Y = owner.Bounds.drawY + pinSelectBounds.drawY + (pinSelectBounds.OuterHeight / 2.0);

            isDirty = true;
        }

        public override void FromBytes(BinaryReader reader)
        {
            base.FromBytes(reader);

            valueInput.SetValue(reader.ReadString());
            valueInput.SetCaretPos(0);
        }

        public override void ToBytes(BinaryWriter writer)
        {
            base.ToBytes(writer);

            writer.Write(valueInput.GetText());
        }

        public void ClearText()
        {
            SetText("");
        }

        public void SetText(string text)
        {
            valueInput.SetValue(text);
        }

        public string GetText()
        {
            return valueInput.GetText();
        }

        public override void OnKeyDown(ICoreClientAPI api, KeyEvent args)
        {
            valueInput.OnKeyDown(api, args);
        }

        public override void OnKeyPress(ICoreClientAPI api, KeyEvent args)
        {
            if (IsKeyAllowed(args.KeyChar))
            {
                valueInput.OnKeyPress(api, args);
            }
        }

        public override bool OnMouseDown(ICoreClientAPI api, MouseEvent mouse)
        {
            var handled = false;

            if (valueInput.IsPositionInside(mouse.X, mouse.Y))
            {
                valueInput.OnFocusGained();
                valueInput.OnMouseDownOnElement(api, mouse);

                handled = true;
            }
            else
            {
                valueInput.OnFocusLost();
            }

            if(cancelButton.IsPositionInside(mouse.X, mouse.Y))
            {
                cancelButton.OnMouseDown(api, mouse);
                cancelButton.OnFocusGained();
            }
            else
            {
                cancelButton.OnFocusLost();
            }

            return handled || pinSelectBounds.PointInside(mouse.X, mouse.Y);
        }

        public override ScriptNodePinConnection CreateConnection()
        {
            if(valueInput.HasFocus || cancelButton.HasFocus)
            {
                return null;
            }
            else return base.CreateConnection();
        }

        public override void OnMouseMove(ICoreClientAPI api, MouseEvent mouse)
        {
            valueInput.OnMouseMove(api, mouse);
            cancelButton.OnMouseMove(api, mouse);
        }

        public override bool OnMouseUp(ICoreClientAPI api, MouseEvent mouse)
        {
            valueInput.OnMouseUp(api, mouse);
            cancelButton.OnMouseUp(api, mouse);

            if (valueInput.IsPositionInside(mouse.X, mouse.Y) == false)
            {
                valueInput.OnFocusLost();
                return false;
            }

            return true;
        }

        public override void Dispose()
        {
            base.Dispose();

            valueInput.Dispose();
            cancelButton.Dispose();
        }

        private void OnTextChanged(string text)
        {
            TextChanged?.Invoke(text);
        }

        private void OnCancelClicked(bool op)
        {
            OnRemoved?.Invoke(this);
        }
    }
}
