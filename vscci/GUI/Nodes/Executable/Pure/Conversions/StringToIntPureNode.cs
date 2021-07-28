namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using System;
    using Vintagestory.API.Client;
    public class StringToIntPureNode : ExecutableScriptNode
    {
        public StringToIntPureNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("String => Int", api, nodeTransform, bounds, true)
        {
            inputs.Add(new ScriptNodeInput(this, "String", typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Int", 1, typeof(int)));
        }

        protected override void OnExecute()
        {
            string input = inputs[0].GetInput();
            try
            {
                outputs[0].Value = int.Parse(input);
            }
            catch(Exception exc)
            {
                api.Logger.Error("Error Converting {0} to Int {1}", input, exc.Message);
                outputs[0].Value = 0;
            }
        }
    }
}
