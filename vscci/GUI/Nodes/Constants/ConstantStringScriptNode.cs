namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes.Attributes;

    [NodeData("Constants", "Constant String")]
    [InputPin(typeof(string), 0)]
    [OutputPin(typeof(string), 0)]
    class ConstantStringScriptNode : ConstantTextInputScriptNode<string>
    {
        public ConstantStringScriptNode(ICoreClientAPI api, MatrixElementBounds bounds) : base(api, bounds)
        {
        }

        public override bool ValidateKey(char key)
        {
            return true;
        }

        protected override string ParseValue(string text)
        {
            return text;
        }

        public override string GetNodeDescription()
        {
            return "This represents a constant string";
        }
    }
}
