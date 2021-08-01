namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using System;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Nodes.Attributes;

    [NodeData("Conversions", "String To Float")]
    [InputPin(typeof(string), 0)]
    [OutputPin(typeof(float), 0)]
    public class StringToFloatPureNode : ExecutableScriptNode
    {
        public StringToFloatPureNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("String => Float", api, nodeTransform, bounds, true)
        {
            inputs.Add(new ScriptNodeInput(this, "String", typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Float", typeof(float)));
        }

        protected override void OnExecute()
        {
            string input = inputs[0].GetInput();
            try
            {
                outputs[0].Value = float.Parse(input);
            }
            catch(Exception exc)
            {
                api.Logger.Error("Error Converting {0} to Float {1}", input, exc.Message);
                outputs[0].Value = 0;
            }
        }
    }
}
