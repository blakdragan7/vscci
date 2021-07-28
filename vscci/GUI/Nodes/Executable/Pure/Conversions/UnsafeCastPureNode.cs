namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using System;
    using Vintagestory.API.Client;
    public class UnsafeCastPureNode<A,B> : ExecutableScriptNode where A : IConvertible
    {
        public UnsafeCastPureNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base($"{typeof(A).Name} > {typeof(B).Name}", api, nodeTransform, bounds, true)
        {
            inputs.Add(new ScriptNodeInput(this, typeof(A).Name, typeof(A)));
            outputs.Add(new ScriptNodeOutput(this, typeof(B).Name, 1, typeof(B)));
        }

        protected override void OnExecute()
        {
            A input = inputs[0].GetInput();
            try
            {
                outputs[0].Value = Convert.ChangeType(input, typeof(B));
            }
            catch(Exception exc)
            {
                api.Logger.Error("Error Casting {0} to {1} {2}", input, typeof(B).Name, exc.Message);
                outputs[0].Value = 0;
            }
        }
    }
}
