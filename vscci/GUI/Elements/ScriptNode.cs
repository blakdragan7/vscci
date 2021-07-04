namespace vscci.GUI.Elements
{
    using Cairo;
    using Vintagestory.API.Client;
    public class ScriptNode : GuiElement
    {
        private Matrix nodeTransform;
        public ScriptNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base(api, bounds)
        {
            this.nodeTransform = nodeTransform;
        }

        public virtual void OnRender(Context ctx, float deltaTime)
        {
            var x = Bounds.drawX;
            var y = Bounds.drawY;

            nodeTransform.TransformPoint(ref x,ref y);

            ctx.SetSourceRGBA(1, 0, 0, 1.0);
            RoundRectangle(ctx, x, y, Bounds.InnerWidth, Bounds.InnerHeight, GuiStyle.ElementBGRadius);
            ctx.Fill();
        }

        public void Move(float deltaX, float deltaY)
        {
            api.Logger.Event("Mouse Mouve {0} {1}", deltaX, deltaY);
            Bounds = Bounds.WithFixedOffset(deltaX, deltaY);
            Bounds.CalcWorldBounds();
        }
    }
}
