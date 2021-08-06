namespace VSCCI.GUI.Elements
{
    using Cairo;
    using Vintagestory.API.Client;

    public class MatrixElementBounds : ElementBounds
    {
        Matrix mat;

        double transformedX;
        double transformedY;

        public override double renderX { get => GetRenderX(); }

        public override double renderY { get => GetRenderY(); }

        private double GetRenderX()
        {
            return transformedX;
        }

        private double GetRenderY()
        {
            return transformedY;
        }

        public static MatrixElementBounds Fixed(int fixedX, int fixedY, Matrix mat)
        {
            return Fixed(fixedX, fixedY, 0, 0, mat);
        }

        public static MatrixElementBounds Fixed(double fixedX, double fixedY, double fixedWidth, double fixedHeight, Matrix mat)
        {
            return new MatrixElementBounds (){ mat = mat, fixedX = fixedX, fixedY = fixedY, fixedWidth = fixedWidth, fixedHeight = fixedHeight, BothSizing = ElementSizing.Fixed };
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

            mat.TransformPoint(ref transformedX, ref transformedY);
        }
    }
}
