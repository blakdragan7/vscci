namespace VSCCI.GUI.Elements
{
    using Cairo;
    using Vintagestory.API.Client;
    using VSCCI.Data;
    using VSCCI.GUI.Elements;

    public class HoverTextElement : GuiElement
    {
        private bool isDirty;
        private TextDrawUtil util;
        private string hoverText;
        private CairoFont font;
        private LoadedTexture backgroundTexture;
        private LoadedTexture textTexture;
        public string Text => hoverText;
        

        public HoverTextElement(ICoreClientAPI api, MatrixElementBounds bounds) : base(api, bounds)
        {
            util = new TextDrawUtil();
            backgroundTexture = new LoadedTexture(api);
            textTexture = new LoadedTexture(api);
            font = CairoFont.WhiteDetailText();
        }

        public void SetHoverText(string text)
        {
            hoverText = text;
            isDirty = true;
        }

        public override void RenderInteractiveElements(float deltaTime)
        {
            if (isDirty)
            {
                isDirty = false;
                ComposeDynamics();
            }
            var matBounds = (MatrixElementBounds)Bounds;

            api.Render.Render2DTexture(backgroundTexture.TextureId, (int)matBounds.untransformedRenderX, (int)matBounds.untransformedRenderY, matBounds.OuterWidthInt, matBounds.OuterHeightInt, Constants.SCRIPT_NODE_HOVER_TEXT_Z_POS);
            api.Render.Render2DTexturePremultipliedAlpha(textTexture.TextureId, (int)matBounds.untransformedRenderX, (int)matBounds.untransformedRenderY, matBounds.OuterWidthInt, matBounds.OuterHeightInt, Constants.SCRIPT_NODE_HOVER_TEXT_Z_POS);
        }

        public void ComposeDynamics()
        {
            RenderBackground();
            RenderText();
        }

        public void SetPosition(double x, double y)
        {
            Bounds.WithFixedPosition(x, y);
            Bounds.CalcWorldBounds();
        }

        private void RenderBackground()
        {
            ImageSurface surface = new ImageSurface(Format.ARGB32, Bounds.OuterWidthInt, Bounds.OuterHeightInt); ;
            Context ctx = genContext(surface);

            ctx.SetSourceRGBA(0.1568627450980392, 0.0980392156862745, 0.0509803921568627, 0.7);
            RoundRectangle(ctx, 0, 0, Bounds.OuterWidth, Bounds.OuterHeight, 1);
            ctx.Fill();

            generateTexture(surface, ref backgroundTexture);

            ctx.Dispose();
            surface.Dispose();
        }

        private void RenderText()
        {
            ImageSurface surface = new ImageSurface(Format.ARGB32, Bounds.OuterWidthInt, Bounds.OuterHeightInt); ;
            Context ctx = genContext(surface);

            font.SetupContext(ctx);
            double height = util.AutobreakAndDrawMultilineTextAt(ctx, font, hoverText, 2, 0, Bounds.InnerWidth - 4);
            if (height != Bounds.fixedHeight)
            {
                isDirty = true;
                Bounds.WithFixedHeight(height).CalcWorldBounds();
            }
            generateTexture(surface, ref textTexture);

            ctx.Dispose();
            surface.Dispose();
        }

        public override void Dispose()
        {
            base.Dispose();

            backgroundTexture.Dispose();
            textTexture.Dispose();
        }
    }
}
