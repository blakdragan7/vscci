namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Nodes.Attributes;

    [NodeData("Constants", "Constant String")]
    [InputPin(typeof(string), 0)]
    [OutputPin(typeof(string), 0)]
    class ConstantStringScriptNode : ConstantTextInputScriptNode<string>
    {
        public ConstantStringScriptNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base(api, nodeTransform, bounds)
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
    }
}
