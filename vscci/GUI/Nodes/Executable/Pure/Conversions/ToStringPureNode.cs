namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using System;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Nodes.Attributes;

    [NodeData("Conversions", "To String")]
    [InputPinAttribute(typeof(DynamicType), 0)]
    [OutputPinAttribute(typeof(string), 0)]
    public class ToStringPureNode : ExecutableScriptNode
    {
        ScriptNodeInput input;
        ScriptNodeOutput output;

        public ToStringPureNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("To String", api, nodeTransform, bounds, true)
        {
            input = new ScriptNodeInput(this, "Value", typeof(DynamicType));
            output = new ScriptNodeOutput(this, "String", typeof(string));

            inputs.Add(input);
            outputs.Add(output);
        }

        protected override void OnExecute()
        {
            dynamic value = input.GetInput();
            try
            {
                output.Value = value.ToString();
            }
            catch (Exception exc)
            {
                api.Logger.Error("Error Converting {0} to String {1}", value, exc.Message);
                outputs[0].Value = 0;
            }
        }

        public override string GetNodeDescription()
        {
            return "This will convert any value to a String";
        }
    }
}