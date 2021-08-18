namespace VSCCI.GUI.Elements
{
    using Cairo;
    using Vintagestory.API.Client;

    public class MatrixElementBounds : ElementBounds
    {
        Matrix mat;

        double transformedX;
        double transformedY;

        double transformedDrawX;
        double transformedDrawY;

        public override double renderX => transformedX;

        public override double renderY => transformedY;

        public override double drawX => transformedDrawX;
        public override double drawY => transformedDrawY;

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
            transformedX = base.renderX;
            transformedY = base.renderY;

            this.mat = mat;
            mat.TransformPoint(ref transformedX, ref transformedY);
            return this;
        }

        public override void CalcWorldBounds()
        {
            base.CalcWorldBounds();

            transformedX = base.renderX;
            transformedY = base.renderY;

            transformedDrawX = base.drawX;
            transformedDrawY = base.drawY;

            if (mat is null == false)
            {
                mat.TransformPoint(ref transformedX, ref transformedY);
                mat.TransformPoint(ref transformedDrawX, ref transformedDrawY);
            }
        }
    }
}
