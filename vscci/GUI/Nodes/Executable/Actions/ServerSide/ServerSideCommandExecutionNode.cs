namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Server;

    public class ServerSideCommandExecutable : ServerSideExecutable
    {
        public override void RunServerSide(IServerPlayer player, ICoreServerAPI api, string data)
        {
            var command = data;
            if(command.StartsWith("/") == false)
            {
                command = "/" + data;
            }

            api.InjectConsole(command);
        }
    }

    public class ServerSideCommandExecutionNode : ServerSideExecutableNode<ServerSideCommandExecutable>
    {
        public ServerSideCommandExecutionNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("Run Server Command", api, nodeTransform, bounds)
        {
            inputs.Add(new ScriptNodeTextInput(this, api, typeof(string)));
        }

        protected override void OnExecute()
        {
            data = inputs[1].GetInput();

            base.OnExecute();
        }
    }
}
