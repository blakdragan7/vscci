namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using System;
    using Vintagestory.API.Client;
    public class ToStringPureNode<A> : ExecutableScriptNode
    {
        public ToStringPureNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("To String", api, nodeTransform, bounds, true)
        {
            inputs.Add(new ScriptNodeInput(this, typeof(A).Name, typeof(A)));
            outputs.Add(new ScriptNodeOutput(this, "String", 1, typeof(string)));
        }

        protected override void OnExecute()
        {
            A input = inputs[0].GetInput();
            try
            {
                outputs[0].Value = input.ToString();
            }
            catch(Exception exc)
            {
                api.Logger.Error("Error Converting {0} to String {1}", input, exc.Message);
                outputs[0].Value = 0;
            }
        }
    }
}
