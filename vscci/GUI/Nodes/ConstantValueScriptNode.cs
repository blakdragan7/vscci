namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;

    class ConstantValueScriptNode : ScriptNode
    {
        private ScriptNodeTextInput input;
        public ConstantValueScriptNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("", api, nodeTransform, bounds)
        {
            input = new ScriptNodeTextInput(this, api, typeof(string));

            inputs.Add(input);
            outputs.Add(new ScriptNodeOutput(this, "out", 1, typeof(int)));
        }

        public virtual bool ValidateTextInput(string text)
        {
            return false;
        }


    }
}
