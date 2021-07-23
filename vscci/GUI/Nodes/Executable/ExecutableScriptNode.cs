namespace vscci.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    public class ExecutableScriptNode : ScriptNode
    {
        private bool isPure;

        // if IsPure is true then this node should be executed before output is accessed even without exec pins
        public bool IsPure => isPure;

        public static int INPUT_EXEC_INDEX = 0;
        public static int OUTPUT_EXEC_INDEX = 0;
        public ExecutableScriptNode(string title, ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds, bool isPure = false) : base(title, api, nodeTransform, bounds)
        {
            this.isPure = isPure;
            if (isPure == false)
            {
                inputs.Add(new ExecInputNode(this));
                outputs.Add(new ExecOutputNode(this));
            }
        }

        public ExecutableScriptNode(string title, ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds, bool skipInputExcec, bool isPure) : base(title, api, nodeTransform, bounds)
        {
            this.isPure = isPure;
            if (isPure == false)
            {
                if (skipInputExcec == false)
                {
                    inputs.Add(new ExecInputNode(this));
                }
                outputs.Add(new ExecOutputNode(this));
            }
        }

        public ExecutableScriptNode(string title, string outputName, ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds, bool isPure = false) : base(title, api, nodeTransform, bounds)
        {
            this.isPure = isPure;
            if (isPure == false)
            {
                inputs.Add(new ExecInputNode(this));
                outputs.Add(new ExecOutputNode(this, outputName));
            }
        }

        public ExecutableScriptNode(string title, string outputName, string inputName, ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds, bool isPure = false) : base(title, api, nodeTransform, bounds)
        {
            this.isPure = isPure;
            if (isPure == false)
            {
                inputs.Add(new ExecInputNode(this, inputName));
                outputs.Add(new ExecOutputNode(this, outputName));
            }
        }

        public virtual void Execute()
        {
            if(isPure == false)ExecuteNextNode();
        }

        public void ExecuteNextNode()
        {
            if(isPure)
            {
                throw new System.Exception("Can not execute next node on pure nodes");
            }

            var connection = outputs[OUTPUT_EXEC_INDEX].TopConnection();
            if (connection != null && connection.IsConnected)
            {
                var exec = connection.Input as ExecInputNode;
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
