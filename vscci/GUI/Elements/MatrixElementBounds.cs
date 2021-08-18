namespace VSCCI.GUI.Elements
{
    using Cairo;
    using Vintagestory.API.Client;

    public class MatrixElementBounds : ElementBounds
    {
        Matrix mat;

        double transformedX;
        double transformedY;

        double transformedRenderX;
        double transformedRenderY;

        double transformedDrawX;
        double transformedDrawY;

        public override double renderX => transformedRenderX;

        public override double renderY => transformedRenderY;

        public override double drawX => transformedDrawX;
        public override double drawY => transformedDrawY;

        public override double absX => transformedX;
        public override double absY => transformedY;

        public virtual double untransformedAbsY => base.absY;
        public virtual double untransformedAbsX => base.absX;

        public virtual double untransformedRenderX => base.renderX;
        public virtual double untransformedRenderY => base.renderY;

        public static MatrixElementBounds Fixed(int fixedX, int fixedY, Matrix mat)
        {
            return Fixed(fixedX, fixedY, 0, 0, mat);
        }

        public static MatrixElementBounds Fixed(double fixedX, double fixedY, double fixedWidth, double fixedHeight, Matrix mat)
        {
            return new MatrixElementBounds (){ mat = mat, fixedX = fixedX, fixedY = fixedY, fixedWidth = fixedWidth, fixedHeight = fixedHeight, BothSizing = ElementSizing.Fixed };
        }

        public static MatrixElementBounds WithBounds(ElementBounds bounds, Matrix mat)
        {
            return new MatrixElementBounds() { mat = mat, fixedX = bounds.fixedX, fixedY = bounds.fixedY, fixedWidth = bounds.fixedWidth, fixedHeight = bounds.fixedHeight, BothSizing = ElementSizing.Fixed };
        }

        public MatrixElementBounds WithMatrix(Matrix mat)
        {
            transformedRenderX = base.renderX;
            transformedRenderY = base.renderY;

            this.mat = mat;
            mat.TransformPoint(ref transformedRenderX, ref transformedRenderY);
            return this;
        }

        public override void CalcWorldBounds()
        {
            base.CalcWorldBounds();

            transformedRenderX = base.renderX;
            transformedRenderY = base.renderY;

            transformedDrawX = base.drawX;
            transformedDrawY = base.drawY;

            transformedX = base.absX;
            transformedY = base.absY;

            if (mat is null == false)
            {
                mat.TransformPoint(ref transformedRenderX, ref transformedRenderY);
                mat.TransformPoint(ref transformedDrawX, ref transformedDrawY);
                mat.TransformPoint(ref transformedX, ref transformedY);
            }
        }
    }
}
