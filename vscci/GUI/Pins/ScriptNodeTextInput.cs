namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;

    public class ScriptNodeTextInput : ScriptNodeInput
    {
        protected GuiElementTextInput textInput;

        public Func<char, bool> IsKeyAllowed;
        public event Action<string> TextChanged;

        public ScriptNodeTextInput(ScriptNode owner, ICoreClientAPI api,  System.Type pinType) : base(owner, "", pinType)
        {
            textInput = new GuiElementTextInput(api, ElementBounds.Empty, OnTextChanged, CairoFont.TextInput());
            // default to true
            IsKeyAllowed = (char c) => { return true; };
            allowsConnections = false;
        }

        public override void RenderOther(Context ctx, ImageSurface surface, double deltaTime)
        {
            var bounds = textInput.Bounds;
            ctx.SetSourceRGBA(0.1, 0.1, 0.1, 0.5);
            RoundRectangle(ctx, X, Y, bounds.InnerWidth, bounds.InnerHeight, 1.0);
            ctx.Fill();
        }

        public override void RenderText(TextDrawUtil textUtil, CairoFont font, Context ctx, ImageSurface surface, double deltaTime)
        {
            textInput.RenderInteractiveElements((float)deltaTime);
        }

        public override void Compose(double x, double y, Context ctx, CairoFont font)
        {
            X = x;
            Y = y;

            owner.Bounds.ParentBounds.ChildBounds.Remove(textInput.Bounds);
            textInput.Bounds = ElementBounds.Fixed(X, Y, 100, 30);
            owner.Bounds.ParentBounds.WithChild(textInput.Bounds);
            textInput.Bounds.CalcWorldBounds();

            extents.Width = textInput.Bounds.OuterWidth;
            extents.Height = textInput.Bounds.OuterHeight;

            isDirty = false;
            pinSelectBounds = textInput.Bounds;

            textInput.Font = font;
            textInput.ComposeElements(ctx, null);
        }

        // do nothing because no pin is needed
        public override void RenderPin(Context ctx, ImageSurface surface, double deltaTime)
        {
            
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
            if (IsKeyAllowed(args.KeyChar))
            {
                textInput.OnKeyDown(api, args);
            }
        }

        public override void OnKeyPress(ICoreClientAPI api, KeyEvent args)
        {
            if (IsKeyAllowed(args.KeyChar))
            {
                textInput.OnKeyPress(api, args);
            }
        }

        public override void OnMouseDown(ICoreClientAPI api, double x, double y, EnumMouseButton button)
        {
            textInput.OnFocusGained();

            var mevent = new MouseEvent((int)x, (int)y, button);
            textInput.OnMouseDownOnElement(api, mevent);
        }

        public override void OnMouseMove(ICoreClientAPI api, double x, double y, double deltaX, double deltaY)
        {
            var mevent = new MouseEvent((int)x, (int)y, (int)deltaX, (int)deltaY);
            textInput.OnMouseMove(api, mevent);
        }
        public override void OnMouseUp(ICoreClientAPI api, double x, double y, EnumMouseButton button)
        {
            var mevent = new MouseEvent((int)x, (int)y, button);
            textInput.OnMouseUp(api, mevent);
            if (PointIsWithinSelectionBounds(x, y) == false)
            {
                textInput.OnFocusLost();
            }
        }

        private void OnTextChanged(string text)
        {
            TextChanged?.Invoke(text);
        }
    }
}
