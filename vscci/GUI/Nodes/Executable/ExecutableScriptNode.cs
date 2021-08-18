namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Pins;

    public abstract class ExecutableScriptNode : ScriptNode
    {
        private bool isPure;

        protected bool shouldAutoExecuteNext;
        protected int nextExecutableIndex;

        // if IsPure is true then this node should be executed before output is accessed even without exec pins
        public bool IsPure => isPure;

        public static int INPUT_EXEC_INDEX = 0;
        public static int OUTPUT_EXEC_INDEX = 0;
        public ExecutableScriptNode(string title, ICoreClientAPI api, MatrixElementBounds bounds, bool isPure = false) : base(title, api, bounds)
        {
            this.isPure = isPure;
            if (isPure == false)
            {
                inputs.Add(new ExecInputNode(this));
                outputs.Add(new ExecOutputNode(this));
            }

            shouldAutoExecuteNext = true;
            nextExecutableIndex = OUTPUT_EXEC_INDEX;
        }

        public ExecutableScriptNode(string title, ICoreClientAPI api, MatrixElementBounds bounds, bool skipInputExcec, bool isPure) : base(title, api, bounds)
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

            shouldAutoExecuteNext = true;
            nextExecutableIndex = OUTPUT_EXEC_INDEX;
        }

        public ExecutableScriptNode(string title, string outputName, ICoreClientAPI api, MatrixElementBounds bounds, bool isPure = false) : base(title, api, bounds)
        {
            this.isPure = isPure;
            if (isPure == false)
            {
                inputs.Add(new ExecInputNode(this));
                outputs.Add(new ExecOutputNode(this, outputName));
            }

            shouldAutoExecuteNext = true;
            nextExecutableIndex = OUTPUT_EXEC_INDEX;
        }

        public ExecutableScriptNode(string title, string outputName, string inputName, ICoreClientAPI api, MatrixElementBounds bounds, bool isPure = false) : base(title, api, bounds)
        {
            this.isPure = isPure;
            if (isPure == false)
            {
                inputs.Add(new ExecInputNode(this, inputName));
                outputs.Add(new ExecOutputNode(this, outputName));
            }

            shouldAutoExecuteNext = true;
            nextExecutableIndex = OUTPUT_EXEC_INDEX;
        }

        public virtual void PrepareExecute()
        {
            if (isPure == false)
            {
                if (inputs.Count > 0)
                {
                    var input = inputs[INPUT_EXEC_INDEX] as ExecInputNode;
                    api.Event.EnqueueMainThreadTask(() => input?.SetPinRunning(true), "Exec Pin");
                }
            }
        }

        public virtual void FinishExecute()
        {
            if (isPure == false)
            {
                if (inputs.Count > 0)
                {
                    var input = inputs[INPUT_EXEC_INDEX] as ExecInputNode;
                    api.Event.EnqueueMainThreadTask(() => input?.SetPinRunning(false), "Exec Pin");
                }
            }
        }

        public void Execute()
        {
            if(isPure == false) PrepareExecute();
            OnExecute();
            if (isPure == false) FinishExecute();

            ExecuteNodeAtIndex(nextExecutableIndex);
        }

        protected abstract void OnExecute();

        public void ExecuteNextNode()
        {
            if(isPure || shouldAutoExecuteNext == false)
            {
                return;
            }

            var connection = outputs[OUTPUT_EXEC_INDEX].TopConnection();
            if (connection != null && connection.IsConnected)
            {
                var exec = connection.Input as ExecInputNode;
                exec?.ExecOwner.Execute();
            }
        }

        public void ExecuteOutput(ScriptNodeOutput output)
        {
            var connection = output.TopConnection();
            if (connection != null && connection.IsConnected)
            {
                var exec = connection.Input as ExecInputNode;
                exec?.ExecOwner.Execute();
            }
        }

        public void ExecuteNodeAtIndex(int index)
        {
            if (isPure || shouldAutoExecuteNext == false)
            {
                return;
            }

            var connection = outputs[index].TopConnection();
            if (connection != null && connection.IsConnected)
            {
                var exec = connection.Input as ExecInputNode;
                exec?.ExecOwner.Execute();
            }
        }
    }
}
