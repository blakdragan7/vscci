namespace VSCCI.GUI.Elements
{
    using Cairo;
    using Vintagestory.API.Client;

    public class HoverTextElement : GuiElement
    {
        private bool isDirty;
        private TextDrawUtil util;
        private string hoverText;
        private CairoFont font;
        public string Text => hoverText;
        

        public HoverTextElement(ICoreClientAPI api, ElementBounds bounds) : base(api, bounds)
        {
            util = new TextDrawUtil();
        }

        public void SetHoverText(string text)
        {
            hoverText = text;
            isDirty = true;
        }

        public void OnRender(Context ctx, ImageSurface surface, float deltaTime)
        {
            if(isDirty)
            {
                OnCompose(ctx);
            }

            RenderBackground(ctx);
            RenderText(ctx);
        }

        public virtual void OnCompose(Context ctx)
        {
            isDirty = false;
            font = CairoFont.WhiteDetailText();
        }

        public void SetPosition(double x, double y)
        {
            Bounds.WithFixedPosition(x, y);
            Bounds.CalcWorldBounds();
        }

        private void RenderBackground(Context ctx)
        {
            ctx.SetSourceRGBA(0.1568627450980392, 0.0980392156862745, 0.0509803921568627, 0.7);
            ElementRoundRectangle(ctx, Bounds, true);
            ctx.Fill();
        }

        private void RenderText(Context ctx)
        {
            ctx.Save();
            font.SetupContext(ctx);
            double height = util.AutobreakAndDrawMultilineTextAt(ctx, font, hoverText, Bounds.drawX + 2, Bounds.drawY, Bounds.InnerWidth - 4);
            if(height != Bounds.fixedHeight)Bounds.WithFixedHeight(height).CalcWorldBounds();
            ctx.Restore();

        }
    }
}
