namespace VSCCI.GUI.Elements
{
    using Cairo;
    using Vintagestory.API.Client;
    using System;
    using VSCCI.Data;
    using VSCCI.GUI.Nodes;

    public class DragSelectBox : GuiElement
    {
        private double startX;
        private double startY;

        private LoadedTexture texture;


        private bool isDirty;
        public DragSelectBox(ICoreClientAPI api, MatrixElementBounds bounds) : base(api, bounds)
        {
            startX = 0;
            startY = 0;

            texture = new LoadedTexture(api);
            isDirty = true;
        }

        public void SetStartPosition(double x, double y)
        {
            startX = x - Bounds.ParentBounds.absX;
            startY = y - Bounds.ParentBounds.absY;

            Bounds.WithFixedPosition(startX, startY);
            Bounds.CalcWorldBounds();

            isDirty = true;
        }

        public void SetEndPosition(double x, double y)
        {
            var relX = x - Bounds.ParentBounds.absX;
            var relY = y - Bounds.ParentBounds.absY;

            var width = relX - startX;
            var height = relY - startY;

            var xInverse = false;
            var yInverse = false;

            if (width < 0)
            {
                width = Math.Abs(width);
                xInverse = true;
            }

            if (height < 0)
            {
                height = Math.Abs(height);
                yInverse = true;
            }

            Bounds.WithFixedSize(width, height);
            Bounds.WithFixedPosition(xInverse ? relX : startX, yInverse ? relY : startY);
            Bounds.CalcWorldBounds();

            isDirty = true;
        }

        public bool NodeIntersects(ScriptNode node)
        {
            var x1 = node.Bounds.absX;
            var y1 = node.Bounds.absY;
            var x2 = node.Bounds.absX + node.Bounds.OuterWidthInt;
            var y2 = node.Bounds.absY + node.Bounds.OuterHeightInt;

            return Bounds.PointInside(x1, y1) || Bounds.PointInside(x2, y2) ||
                   Bounds.PointInside(x1, y2) || Bounds.PointInside(x2, y1) || IntersectsNode(node);
        }

        private bool IntersectsNode(ScriptNode node)
        {
            var x1 = Bounds.absX;
            var y1 = Bounds.absY;
            var x2 = Bounds.absX + Bounds.OuterWidthInt;
            var y2 = Bounds.absY + Bounds.OuterHeightInt;

            return node.Bounds.PointInside(x1, y1) || node.Bounds.PointInside(x2, y2) ||
                   node.Bounds.PointInside(x1, y2) || node.Bounds.PointInside(x2, y1);
        }

        public override void OnMouseMove(ICoreClientAPI api, MouseEvent args)
        {
            base.OnMouseMove(api, args);

            SetEndPosition(args.X, args.Y);
        }

        public override void RenderInteractiveElements(float deltaTime)
        {
            base.RenderInteractiveElements(deltaTime);

            if(isDirty)
            {
                ComposeTexture();
            }

            var matBounds = (MatrixElementBounds)Bounds;
            api.Render.Render2DTexture(texture.TextureId, (float)matBounds.untransformedRenderX, (float)matBounds.untransformedRenderY,
                matBounds.OuterWidthInt, matBounds.OuterHeightInt, Constants.SCRIPT_NODE_HOVER_TEXT_Z_POS);
        }

        private void ComposeTexture()
        {
            var surface = new ImageSurface(Format.Argb32, Bounds.OuterWidthInt, Bounds.OuterHeightInt);
            var ctx = new Context(surface);

            ctx.SetSourceRGBA(1.0, 1.0, 1.0, 0.5);
            RoundRectangle(ctx, 0, 0, Bounds.OuterWidth, Bounds.OuterHeight, 1);
            ctx.Fill();

            generateTexture(surface, ref texture);

            ctx.Dispose();
            surface.Dispose();

            isDirty = false;
        }
    }
}
