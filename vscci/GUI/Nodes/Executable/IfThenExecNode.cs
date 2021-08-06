namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes.Attributes;

    [NodeData("Flow", "If Then")]
    [InputPin(typeof(Exec), 0)]
    [InputPin(typeof(bool), 1)]
    [OutputPin(typeof(Exec), 0)]
    public class IfThenExecNode : ExecutableScriptNode
    {
        public static int CONDITION_INPUT_INDEX = 1;
        public static int FALSE_OUTPUT_INDEX = 1;
        public IfThenExecNode(ICoreClientAPI api, MatrixElementBounds bounds) : base("If / Then", "True", api, bounds)
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

        public override string GetNodeDescription()
        {
            return "If \"condition\" is true, the \"True\" path is executed, Otherwise the \"False\" path is executed.";
        }
    }
}
