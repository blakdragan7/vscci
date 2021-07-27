namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    public class IfThenExecNode : ExecutableScriptNode
    {
        public static int CONDITION_INPUT_INDEX = 1;
        public static int FALSE_OUTPUT_INDEX = 1;
        public IfThenExecNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("If / Then", "True", api, nodeTransform, bounds)
        {
            inputs.Add(new ScriptNodeInput(this, "condition", typeof(bool)));
            outputs.Add(new ExecOutputNode(this, "False"));
        }

        protected override void OnExecute()
        {
            if (inputs[CONDITION_INPUT_INDEX].TopConnection() != null)
            {
                bool condition = inputs[CONDITION_INPUT_INDEX].GetInput();
                if(condition == false)
                {
                    shouldAutoExecuteNext = false;
                    nextExecutableIndex = FALSE_OUTPUT_INDEX;
                }
            }
        }
    }
}
