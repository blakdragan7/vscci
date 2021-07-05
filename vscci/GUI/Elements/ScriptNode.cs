namespace vscci.GUI.Elements
{
    using Cairo;
    using Vintagestory.API.Client;
    using System.Collections.Generic;
    using vscci.Data;

    public class ScriptNode : GuiElement
    {
        private Matrix nodeTransform;
        private List<string> inputs;
        private List<string> outputs;
        private TextDrawUtil textUtil;
        private CairoFont font;

        public ScriptNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base(api, bounds)
        {
            textUtil = new TextDrawUtil();
            this.nodeTransform = nodeTransform;
            font = CairoFont.WhiteDetailText().WithFontSize(15);

            inputs = new List<string>()
            {
                "exec",
                "Some In"
            };
            outputs = new List<string>()
            {
                "exec",
                "Some Out"
            };
        }

        public virtual void OnRender(Context ctx, float deltaTime)
        {
            var x = Bounds.drawX;
            var y = Bounds.drawY;

            nodeTransform.TransformPoint(ref x,ref y);

            ctx.SetSourceRGBA(1, 0, 0, 1.0);
            RoundRectangle(ctx, x, y, Bounds.InnerWidth, Bounds.InnerHeight, GuiStyle.ElementBGRadius);
            ctx.Fill();

            ctx.SetSourceRGBA(1.0, 1.0, 1.0, 1.0);
            ctx.Save();
            font.SetupContext(ctx);

            var startDrawY = y;

            var bigestWidth = 0.0d;
            foreach (var text in inputs)
            {
                var extentes = ctx.TextExtents(text);
                textUtil.DrawTextLine(ctx, font, text, x, y);

                y += extentes.Height + Constants.NODE_SCIPRT_TEXT_PADDING;
                bigestWidth = bigestWidth > extentes.Width ? bigestWidth : extentes.Width;
            }

            x += bigestWidth + Constants.NODE_SCIPRT_TEXT_PADDING;
            y = startDrawY;

            foreach (var text in outputs)
            {
                var extentes = ctx.TextExtents(text);
                textUtil.DrawTextLine(ctx, font, text, x, y);

                y += extentes.Height + Constants.NODE_SCIPRT_TEXT_PADDING;
                bigestWidth = bigestWidth > extentes.Width ? bigestWidth : extentes.Width;
            }

            ctx.Restore();
        }

        public void Move(float deltaX, float deltaY)
        {
            api.Logger.Event("Mouse Mouve {0} {1}", deltaX, deltaY);
            Bounds = Bounds.WithFixedOffset(deltaX, deltaY);
            Bounds.CalcWorldBounds();
        }
    }
}
