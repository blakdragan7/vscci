namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using System.Collections.Generic;
    using System;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes.Attributes;
    using VSCCI.GUI.Pins;

    [NodeData("Constants", "Constant Number")]
    [InputPin(typeof(Number), 0)]
    [OutputPin(typeof(Number), 0)]
    class ConstantNumberScriptNode : ScriptNode
    {
        public ConstantNumberScriptNode(ICoreClientAPI api, MatrixElementBounds bounds) : base("", api, bounds)
        {
            inputs.Add(new ScriptNodeNumberInput(this, InputUpdated, ""));
            outputs.Add(new ScriptNodeOutput(this, "Out", typeof(Number)));
        }

        private void InputUpdated(string _)
        {
            outputs[0].Value = inputs[0].GetInput();
        }

        public override string GetNodeDescription()
        {
            return "This represents a constant number";
        }
    }
}
