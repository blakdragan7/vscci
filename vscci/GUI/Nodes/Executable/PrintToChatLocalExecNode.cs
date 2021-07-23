namespace vscci.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    class PrintToChatLocalExecNode : ExecutableScriptNode
    {
        public static int MESSAGE_INPUT_INDEX = 1;
        public PrintToChatLocalExecNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("Print To Chat", api, nodeTransform, bounds)
        {
            inputs.Add(new ScriptNodeInput(this, "message", typeof(string)));
        }

        public override void Execute()
        {
            string message = inputs[MESSAGE_INPUT_INDEX].GetInput();

            if(message != null)
            {
                api.Event.EnqueueMainThreadTask(() => api.ShowChatMessage(message) , "Print To Chat Local");
            }

            ExecuteNextNode();
        }
    }
}
