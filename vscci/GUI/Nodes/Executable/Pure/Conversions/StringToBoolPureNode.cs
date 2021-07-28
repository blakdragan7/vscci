namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using System;
    using Vintagestory.API.Client;
    public class StringToBoolPureNode : ExecutableScriptNode
    {
        public StringToBoolPureNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("String => Bool", api, nodeTransform, bounds, true)
        {
            inputs.Add(new ScriptNodeInput(this, "String", typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Bool", 1, typeof(bool)));
        }

        protected override void OnExecute()
        {
            string input = inputs[0].GetInput();
            try
            {
                outputs[0].Value = bool.Parse(input);
            }
            catch(Exception exc)
            {
                api.Logger.Error("Error Converting {0} to Bool {1}", input, exc.Message);
                outputs[0].Value = 0;
            }
        }
    }
}
