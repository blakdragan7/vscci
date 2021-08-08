namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes.Attributes;
    using VSCCI.GUI.Pins;

    [NodeData("Basic", "<=")]
    [InputPin(typeof(Number), 0)]
    [InputPin(typeof(Number), 1)]
    [OutputPin(typeof(bool), 0)]
    class LessOrEqualPureNode : ExecutableScriptNode
    {
        public LessOrEqualPureNode(ICoreClientAPI api, MatrixElementBounds bounds) : base("<=", api, bounds, true)
        {
            inputs.Add(new ScriptNodeInput(this, "First", typeof(Number)));
            inputs.Add(new ScriptNodeInput(this, "Second", typeof(Number)));

            outputs.Add(new ScriptNodeOutput(this, "Result", typeof(bool)));

            shouldAutoExecuteNext = false;
        }

        protected override void OnExecute()
        {
            dynamic first = inputs[0].GetInput();
            dynamic second = inputs[1].GetInput();

            outputs[0].Value = first <= second;
        }

        public override string GetNodeDescription()
        {
            return "This will set \"Result\" to true if \"First\" is less then or equal to \"Second\"";
        }
    }
}
