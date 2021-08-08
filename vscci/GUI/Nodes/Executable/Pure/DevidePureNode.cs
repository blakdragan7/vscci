namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes.Attributes;
    using VSCCI.GUI.Pins;

    [NodeData("Basic", "/")]
    [InputPin(typeof(Number), 0)]
    [InputPin(typeof(Number), 1)]
    [OutputPin(typeof(Number), 0)]
    public class DevidePureNode : ExecutableScriptNode
    {
        public static int INPUT_ONE_INDEX = 0;
        public static int INPUT_TWO_INDEX = 1;
        public static int OUTPUT_INDEX = 0;
        public DevidePureNode(ICoreClientAPI api, MatrixElementBounds bounds) : base("/", api, bounds, true)
        {
            inputs.Add(new ScriptNodeInput(this, "Top", typeof(Number)));
            inputs.Add(new ScriptNodeInput(this, "Bottom", typeof(Number)));

            outputs.Add(new ScriptNodeOutput(this, "Result", typeof(Number)));

            shouldAutoExecuteNext = false;
        }

        protected override void OnExecute()
        {
            Number first = inputs[INPUT_ONE_INDEX].GetInput();
            Number second = inputs[INPUT_TWO_INDEX].GetInput();

            outputs[OUTPUT_INDEX].Value = first / second;
        }

        public override string GetNodeDescription()
        {
            return "This Devides \"First\" by \"Second\"";
        }
    }
}
