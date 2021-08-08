namespace VSCCI.GUI.Nodes
{
    using Vintagestory.API.Client;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes.Attributes;
    using VSCCI.GUI.Pins;

    [NodeData("Actions", "Show Chat Local")]
    [InputPin(typeof(Exec), 0)]
    [InputPin(typeof(string), 1)]
    [OutputPin(typeof(Exec), 0)]
    class PrintToChatLocalExecNode : ExecutableScriptNode
    {
        public static int MESSAGE_INPUT_INDEX = 1;
        public PrintToChatLocalExecNode(ICoreClientAPI api, MatrixElementBounds bounds) : base("Print To Local Chat", api, bounds)
        {
            inputs.Add(new ScriptNodeInput(this, "Message", typeof(string)));
        }

        protected override void OnExecute()
        {
            string message = inputs[MESSAGE_INPUT_INDEX].GetInput();

            if(message != null)
            {
                api.Event.EnqueueMainThreadTask(() => api.ShowChatMessage(message) , "Print To Chat Local");
            }

            ExecuteNextNode();
        }

        public override string GetNodeDescription()
        {
            return "This Prints to the local chat. Only the current player will see the chat message";
        }
    }
}
