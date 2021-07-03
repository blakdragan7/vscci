namespace vscci.GUI.Elements
{
    using Cairo;
    using Vintagestory.API.Client;
    public class ScriptNode : GuiElement
    {

        public ScriptNode(ICoreClientAPI api, ElementBounds bounds) : base(api, bounds)
        {

        }

        public virtual void OnRender(Context ctx, float deltaTime)
        {
            ctx.SetSourceRGBA(1, 0, 0, 1.0);
            ElementRoundRectangle(ctx, Bounds);
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
