namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Server;
    using VSCCI.Data;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes.Attributes;
    using VSCCI.GUI.Pins;

    public abstract class ServerSideExecutable
    {
        public abstract void RunServerSide(IServerPlayer player, ICoreServerAPI api, string data);
    }

    [InputPin(typeof(Exec), 0)]
    [OutputPin(typeof(Exec), 0)]
    public abstract class ServerSideExecutableNode<T> : ExecutableScriptNode where T : ServerSideExecutable
    {
        private LoadedTexture notAllowedTexture;

        protected string data;

        public ServerSideExecutableNode(string title, ICoreClientAPI api, MatrixElementBounds bounds) : base(title, api, bounds, false)
        {
            notAllowedTexture = new LoadedTexture(api);
        }

        protected override void OnExecute()
        {
            api.Network.GetChannel(Constants.NETWORK_NODE_CHANNEL).SendPacket(new ServerNodeExecutionData()
            {
                AssemblyQualifiedName = typeof(T).AssemblyQualifiedName,
                Data = data
            });
        }

        public override void RenderInteractiveElements(float deltaTime)
        {
            base.RenderInteractiveElements(deltaTime);

            // visually render that this node won't do anything because we don't have permission
            if(ConfigData.clientData.PlayerIsAllowedServerEvents == false)
            {
                
            }
        }

        private void ComposeNotAllowedTexture()
        {
            ImageSurface surface = new ImageSurface(Format.ARGB32, Bounds.OuterWidthInt, Bounds.OuterHeightInt); ;
            Context ctx = genContext(surface);

            var drawX = 0;
            var drawY = 0;

            ctx.Save();
            ctx.SetSourceRGBA(1.0, 0.0, 0.0, 1.0);

            ctx.MoveTo(drawX, drawY);
            ctx.LineTo(drawX + Bounds.OuterWidth, drawY + Bounds.OuterHeight);
            ctx.Stroke();

            ctx.MoveTo(drawX + Bounds.OuterWidth, drawY);
            ctx.LineTo(drawX, drawY + Bounds.OuterHeight);
            ctx.Stroke();

            ctx.Restore();

            generateTexture(surface, ref notAllowedTexture);

            surface.Dispose();
            ctx.Dispose();
        }
    }
}
