namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Server;
    using VSCCI.GUI.Nodes.Attributes;

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

    [NodeData("Actions", "Run Server Command")]
    [InputPin(typeof(string), 1)]
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

        public override string GetNodeDescription()
        {
            return "This Executes a command Server side. Only thise with \"All-Allowed\" Event persmissions can use this node";
        }
    }
}
