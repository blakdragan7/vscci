namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Server;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes.Attributes;
    using VSCCI.Data;
    using VSCCI.GUI.Pins;

    public class ServerSideCommandExecutable : ServerSideAction
    {
        public override void RunServerSide(IServerPlayer player, ICoreServerAPI api, string data)
        {
        }
    }

    [NodeData("Actions", "Run Server Command")]
    [InputPin(typeof(string), 1)]
    public class ServerSideCommandExecutionNode : ServerSideExecutableNode<ServerSideCommandExecutable>
    {
        public ServerSideCommandExecutionNode(ICoreClientAPI api, MatrixElementBounds bounds) : base("Run Server Command", api, bounds)
        {
            inputs.Add(new ScriptNodeTextInput(this, typeof(string)));
        }

        protected override void OnExecute()
        {
            data = "null";
            var command = inputs[1].GetInput();

            if (ConfigData.clientData.PlayerIsAllowedServerEvents)
            {
                if (command.StartsWith("/") == false)
                {
                    command = "/" + data;
                }

                api.SendChatMessage(command);
            }

            base.OnExecute();
        }

        public override string GetNodeDescription()
        {
            return "This Executes a command Server side. Only those with \"All-Allowed\" Event persmissions can use this node";
        }
    }
}
