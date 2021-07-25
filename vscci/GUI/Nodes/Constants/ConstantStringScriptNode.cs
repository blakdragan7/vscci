namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;

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
