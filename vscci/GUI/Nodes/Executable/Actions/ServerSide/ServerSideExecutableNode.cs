namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Server;
    using VSCCI.Data;

    public abstract class ServerSideExecutable
    {
        public abstract void RunServerSide(IServerPlayer player, ICoreServerAPI api, string data);
    }

    public class ServerSideExecutableNode<T> : ExecutableScriptNode where T : ServerSideExecutable
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
    }
}
