namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using System;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Nodes.Attributes;

    [NodeData("Basic", ">")]
    [InputPin(typeof(NumberType), 0)]
    [InputPin(typeof(NumberType), 1)]
    [OutputPin(typeof(bool), 0)]
    class GreaterThenPureNode : ExecutableScriptNode
    {
        public GreaterThenPureNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base(">", api, nodeTransform, bounds, true)
        {
            inputs.Add(new ScriptNodeInput(this, "First", typeof(NumberType)));
            inputs.Add(new ScriptNodeInput(this, "Second", typeof(NumberType)));

            outputs.Add(new ScriptNodeOutput(this, "Result", typeof(bool)));

            shouldAutoExecuteNext = false;
        }

        protected override void OnExecute()
        {
            NumberType first = inputs[0].GetInput();
            NumberType second = inputs[1].GetInput();

            outputs[0].Value = first > second;
        }
    }
}
