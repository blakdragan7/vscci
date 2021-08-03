namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using System;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Nodes.Attributes;

    [NodeData("Basic", "<")]
    [InputPin(typeof(Number), 0)]
    [InputPin(typeof(Number), 1)]
    [OutputPin(typeof(bool), 0)]
    class LessThenPureNode : ExecutableScriptNode
    {
        public LessThenPureNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("<", api, nodeTransform, bounds, true)
        {
            inputs.Add(new ScriptNodeInput(this, "First", typeof(Number)));
            inputs.Add(new ScriptNodeInput(this, "Second", typeof(Number)));

            outputs.Add(new ScriptNodeOutput(this, "Result", typeof(bool)));

            shouldAutoExecuteNext = false;
        }

        protected override void OnExecute()
        {
            Number first = inputs[0].GetInput();
            Number second = inputs[1].GetInput();

            outputs[0].Value = first < second;
        }

        public override string GetNodeDescription()
        {
            return "This will set \"Result\" to true if \"First\" is less then \"Second\"";
        }
    }
}
