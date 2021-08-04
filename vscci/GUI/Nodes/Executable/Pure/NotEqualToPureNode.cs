namespace VSCCI.GUI.Nodes.Executable.Pure
{
    using Cairo;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Nodes.Attributes;

    [NodeData("Basic", "Not Equals")]
    [InputPin(typeof(DynamicType), 0)]
    [InputPin(typeof(DynamicType), 1)]
    [OutputPin(typeof(bool), 0)]
    public class NotEqualToPureNode : ExecutableScriptNode
    {
        public static int INPUT_ONE_INDEX = 0;
        public static int INPUT_TWO_INDEX = 1;
        public static int OUTPUT_INDEX = 0;
        public NotEqualToPureNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("!=", api, nodeTransform, bounds, true)
        {
            inputs.Add(new ScriptNodeInput(this, "First", typeof(DynamicType)));
            inputs.Add(new ScriptNodeInput(this, "Second", typeof(DynamicType)));

            outputs.Add(new ScriptNodeOutput(this, "Result", typeof(bool)));

            shouldAutoExecuteNext = false;
        }

        protected override void OnExecute()
        {
            dynamic first = inputs[INPUT_ONE_INDEX].GetInput();
            dynamic second = inputs[INPUT_TWO_INDEX].GetInput();

            outputs[OUTPUT_INDEX].Value = first != second;
        }

        public override string GetNodeDescription()
        {
            return "This node Compares to Values and sets \"Result\" to True if they are NOT equal";
        }
    }
}
