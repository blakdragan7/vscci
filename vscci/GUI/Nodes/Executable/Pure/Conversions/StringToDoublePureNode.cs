namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using System;
    using Vintagestory.API.Client;
    public class StringToDoublePureNode : ExecutableScriptNode
    {
        public StringToDoublePureNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("String => Double", api, nodeTransform, bounds, true)
        {
            inputs.Add(new ScriptNodeInput(this, "String", typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Int", 1, typeof(double)));
        }

        protected override void OnExecute()
        {
            string input = inputs[0].GetInput();
            try
            {
                outputs[0].Value = double.Parse(input);
            }
            catch(Exception exc)
            {
                api.Logger.Error("Error Converting {0} to Double {1}", input, exc.Message);
                outputs[0].Value = 0;
            }
        }
    }
}
