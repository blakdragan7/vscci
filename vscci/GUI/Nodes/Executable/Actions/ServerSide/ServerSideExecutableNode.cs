namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Server;
    using VSCCI.Data;

    public abstract class ServerSideExecutable
    {

    }

    public abstract class ServerSideExecutableNode : ExecutableScriptNode
    {
        public ServerSideExecutableNode(string title, ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds, bool isPure = false) : base(title, api, nodeTransform, bounds, isPure)
        {
        }

        protected virtual void OnServerSide(IServerPlayer player, int val)
        {

        }

        protected override void OnExecute()
        {
           
        }
    }
}
