namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Server;
    using VSCCI.Data;
    using VSCCI.GUI.Nodes.Attributes;

    public abstract class ServerSideExecutable
    {
        public abstract void RunServerSide(IServerPlayer player, ICoreServerAPI api, string data);
    }

    [InputPin(typeof(Exec), 0)]
    [OutputPin(typeof(Exec), 0)]
    public abstract class ServerSideExecutableNode<T> : ExecutableScriptNode where T : ServerSideExecutable
    {
        protected string data;
        public ServerSideExecutableNode(string title, ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base(title, api, nodeTransform, bounds, false)
        {
        }

        protected override void OnExecute()
        {
            api.Network.GetChannel(Constants.NETWORK_NODE_CHANNEL).SendPacket(new ServerNodeExecutionData()
            {
                AssemblyQualifiedName = typeof(T).AssemblyQualifiedName,
                Data = data
            });
        }

        public override void OnRender(Context ctx, ImageSurface surface, float deltaTime)
        {
            base.OnRender(ctx, surface, deltaTime);

            // visually render that this node won't do anything because we don't have permission
            if(ConfigData.clientData.PlayerIsAllowedServerEvents == false)
            {
                var drawX = cachedRenderX;
                var drawY = cachedRenderY;

                ctx.Save();
                ctx.SetSourceRGBA(1.0, 0.0, 0.0, 1.0);

                ctx.MoveTo(drawX, drawY);
                ctx.LineTo(drawX + Bounds.OuterWidth, drawY + Bounds.OuterHeight);
                ctx.Stroke();

                ctx.MoveTo(drawX + Bounds.OuterWidth, drawY);
                ctx.LineTo(drawX, drawY + Bounds.OuterHeight);
                ctx.Stroke();

                ctx.Restore();
            }
        }
    }
}
