namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;

    public class ScriptNodeTextInput : ScriptNodeInput
    {
        public string Text { get; set; }

        protected ElementBounds bounds;
        public ElementBounds Bounds => bounds;

        public Func<char, bool> IsKeyAllowed;

        public ScriptNodeTextInput(ScriptNode owner, ICoreClientAPI api,  System.Type pinType) : base(owner, "", pinType)
        {
            Text = "test";
            bounds = ElementBounds.Fixed(0, 0);
            // default to true
            IsKeyAllowed = (char c) => { return true; };
        }

        public override void RenderText(TextDrawUtil textUtil, CairoFont font, Context ctx, ImageSurface surface)
        {
            ctx.SetSourceRGBA(1.0, 1.0, 1.0, 1.0);
            textUtil.DrawTextLine(ctx, font, Text, X, Y);

            if(isDirty)
            {
                extents = ctx.TextExtents(Text);
                bounds = ElementBounds.Fixed(X, Y, extents.Width, extents.Height);
                owner.Bounds.ParentBounds.WithChild(bounds);
                bounds.CalcWorldBounds();

                isDirty = false;
            }
        }

        // do nothing because no pin is needed
        public override void RenderPin(Context ctx, ImageSurface surface)
        {
            ctx.SetSourceRGBA(0.9,0.9,0.9,1.0);
            RoundRectangle(ctx, X, Y, bounds.InnerWidth, bounds.InnerHeight, 1.0);
            ctx.Fill();
        }

        public void ClearText()
        {
            SetText("");
        }

        public void SetText(string text)
        {
            Text = text;
            MarkDirty();
        }

        public void OnKeyPress(ICoreClientAPI api, KeyEvent args)
        {
            if (IsKeyAllowed(args.KeyChar))
            {
                Text += args.KeyChar;
                MarkDirty();
            }
        }

        public void OnMouseDown(ICoreClientAPI api, double x, double y, EnumMouseButton button)
        {

        }

        public void OnMouseMove(double deltaX, double deltaY)
        {

        }

        public void OnMouseUp(ICoreClientAPI api, double x, double y, EnumMouseButton button)
        {

        }
    }
}
