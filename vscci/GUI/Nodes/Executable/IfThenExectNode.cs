namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    public class IfThenExectNode : ExecutableScriptNode
    {
        public static int CONDITION_INPUT_INDEX = 1;
        public static int FALSE_OUTPUT_INDEX = 1;
        public IfThenExectNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("If / Then", "True", api, nodeTransform, bounds)
        {
            inputs.Add(new ScriptNodeInput(this, "condition", typeof(bool)));
            outputs.Add(new ExecOutputNode(this, "False"));
        }

        public override void Execute()
        {
            if (inputs[CONDITION_INPUT_INDEX].TopConnection() != null)
            {
                bool condition = inputs[CONDITION_INPUT_INDEX].GetInput();
                if(condition)
                {
                    ExecuteNextNode();
                }
                else
                {
                    ExecuteNodeAtIndex(FALSE_OUTPUT_INDEX);
                }
            }
            else
            {
                ExecuteNextNode();
            }
        }
    }
}
