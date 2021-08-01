namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;

    public abstract class ConstantTextInputScriptNode<T> : ScriptNode
    {
        private ScriptNodeTextInput input;

        private ScriptNodeOutput output;

        public ConstantTextInputScriptNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("", api, nodeTransform, bounds)
        {
            input = new ScriptNodeTextInput(this, api, typeof(T));
            output = new ScriptNodeOutput(this, "out", typeof(T));

            inputs.Add(input);
            outputs.Add(output);

            input.TextChanged += OnTextChanged;
            input.IsKeyAllowed += ValidateKey;
        }

        public void OnTextChanged(string text)
        {
            output.Value = ParseValue(text);
        }

        public virtual bool ValidateKey(char key)
        {
            return true;
        }

        protected abstract T ParseValue(string text);
    }
}
