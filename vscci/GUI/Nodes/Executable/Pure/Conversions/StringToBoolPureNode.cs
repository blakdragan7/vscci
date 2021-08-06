namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using System;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes.Attributes;

    [NodeData("Conversions", "String To Bool")]
    [InputPin(typeof(string), 0)]
    [OutputPin(typeof(bool), 0)]
    public class StringToBoolPureNode : ExecutableScriptNode
    {
        public StringToBoolPureNode(ICoreClientAPI api, MatrixElementBounds bounds) : base("String => Bool", api, bounds, true)
        {
            inputs.Add(new ScriptNodeInput(this, "String", typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Bool", typeof(bool)));
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
                outputs[0].Value = false;
            }
        }

        public override string GetNodeDescription()
        {
            return "If \"String\" is True or true then \"Bool\" will be true. Otherwise it will be false";
        }
    }
}
