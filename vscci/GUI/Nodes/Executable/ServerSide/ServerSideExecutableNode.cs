namespace vscci.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Server;
    using vscci.Data;

    class ServerSideExecutableNode : ExecutableScriptNode
    {
        public ServerSideExecutableNode(string title, ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds, bool isPure = false) : base(title, api, nodeTransform, bounds, isPure)
        {
        }

        protected virtual void OnServerSide(IServerPlayer player, int val)
        {

        }
    }
}
