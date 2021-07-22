namespace vscci.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    public class ExecutableScriptNode : ScriptNode
    {
        public static int INPUT_EXEC_INDEX = 0;
        public static int OUTPUT_EXEC_INDEX = 0;
        public ExecutableScriptNode(string title, ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base(title, api, nodeTransform, bounds)
        {
            inputs.Add(new ExecInputNode(this));
            outputs.Add(new ExecOutputNode(this));
        }

        public ExecutableScriptNode(string title, ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds, bool skipInputExcec) : base(title, api, nodeTransform, bounds)
        {
            if (skipInputExcec == false)
            {
                inputs.Add(new ExecInputNode(this));
            }
            outputs.Add(new ExecOutputNode(this));
        }

        public ExecutableScriptNode(string title, string outputName, ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base(title, api, nodeTransform, bounds)
        {
            inputs.Add(new ExecInputNode(this));
            outputs.Add(new ExecOutputNode(this, outputName));
        }

        public ExecutableScriptNode(string title, string outputName, string inputName, ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base(title, api, nodeTransform, bounds)
        {
            inputs.Add(new ExecInputNode(this, inputName));
            outputs.Add(new ExecOutputNode(this, outputName));
        }

        public virtual void Execute()
        {
            ExecuteNextNode();
        }

        public void ExecuteNextNode()
        {
            var connection = outputs[OUTPUT_EXEC_INDEX].TopConnection();
            if (connection != null && connection.IsConnected)
            {
                var exec = connection.Output as ExecOutputNode;
                exec?.ExecOwner.Execute();
            }
        }

        public void ExecuteNodeAtIndex(int index)
        {
            var connection = outputs[index].TopConnection();
            if (connection != null && connection.IsConnected)
            {
                var exec = connection.Output as ExecOutputNode;
                exec?.ExecOwner.Execute();
            }
        }
    }
}
