namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Nodes.Attributes;

    [NodeData("Basic", "-")]
    [InputPin(typeof(NumberType), 0)]
    [InputPin(typeof(NumberType), 1)]
    [OutputPin(typeof(NumberType), 0)]
    public class SubtractPureNode : ExecutableScriptNode
    {
        public static int INPUT_ONE_INDEX = 0;
        public static int INPUT_TWO_INDEX = 1;
        public static int OUTPUT_INDEX = 0;
        public SubtractPureNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("-", api, nodeTransform, bounds, true)
        {
            inputs.Add(new ScriptNodeInput(this, "First", typeof(NumberType)));
            inputs.Add(new ScriptNodeInput(this, "Second", typeof(NumberType)));

            outputs.Add(new ScriptNodeOutput(this, "Result", typeof(NumberType)));

            shouldAutoExecuteNext = false;
        }

        protected override void OnExecute()
        {
            NumberType first = inputs[INPUT_ONE_INDEX].GetInput();
            NumberType second = inputs[INPUT_TWO_INDEX].GetInput();

            outputs[OUTPUT_INDEX].Value = first - second;
        }
    }
}
