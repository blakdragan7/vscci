namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using System;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Nodes.Attributes;

    [NodeData("Conversions", "String To Int")]
    [InputPin(typeof(string), 0)]
    [OutputPin(typeof(NumberType), 0)]
    public class StringToNumberPureNode : ExecutableScriptNode
    {
        public StringToNumberPureNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("String => Number", api, nodeTransform, bounds, true)
        {
            inputs.Add(new ScriptNodeInput(this, "String", typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Number", typeof(NumberType)));
        }

        protected override void OnExecute()
        {
            string input = inputs[0].GetInput();
            outputs[0].Value = NumberType.Parse(input);
        }
    }
}
