namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;

    class ConstantValueScriptNode : ScriptNode
    {
        private ScriptNodeTextInput input;
        private ScriptNodeOutput output;

        public ConstantValueScriptNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("", api, nodeTransform, bounds)
        {
            input = new ScriptNodeTextInput(this, api, typeof(string));
            output = new ScriptNodeOutput(this, "out", 1, typeof(string));

            inputs.Add(input);
            outputs.Add(output);

            input.TextChanged += OnTextChanged;
        }

        public void OnTextChanged(string text)
        {
            output.Value = text;
        }

        public virtual bool ValidateTextInput(string text)
        {
            return false;
        }
    }
}
