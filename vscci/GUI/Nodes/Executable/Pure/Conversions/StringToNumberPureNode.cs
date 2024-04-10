namespace VSCCI.GUI.Nodes
{
    using Vintagestory.API.Client;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes.Attributes;
    using VSCCI.GUI.Pins;

    [NodeData("Conversions", "String To Int")]
    [InputPin(typeof(string), 0)]
    [OutputPin(typeof(Number), 0)]
    public class StringToNumberPureNode : ExecutableScriptNode
    {
        public StringToNumberPureNode(ICoreClientAPI api, MatrixElementBounds bounds) : base("String => Number", api, bounds, true)
        {
            inputs.Add(new ScriptNodeInput(this, "String", typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Number", typeof(Number)));
        }

        protected override void OnExecute()
        {
            string input = inputs[0].GetInput();
            outputs[0].Value = Number.Parse(input);
        }

        public override string GetNodeDescription()
        {
            return "This converts \"String\" to a Number if possible. If not, it sets \"Number\" to 0";
        }
    }
}
