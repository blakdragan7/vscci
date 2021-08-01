namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using System;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Nodes.Attributes;

    public class UnsafeCastPureNode<A,B> : ExecutableScriptNode where A : IConvertible
    {
        public UnsafeCastPureNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base($"{typeof(A).Name} > {typeof(B).Name}", api, nodeTransform, bounds, true)
        {
            inputs.Add(new ScriptNodeInput(this, typeof(A).Name, typeof(A)));
            outputs.Add(new ScriptNodeOutput(this, typeof(B).Name, typeof(B)));
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

    [NodeData("Conversions", "Int To Float")]
    [InputPin(typeof(int), 0)]
    [OutputPin(typeof(float), 0)]
    public class IntToFloat : UnsafeCastPureNode<int, float>
    {
        public IntToFloat(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base(api, nodeTransform, bounds)
        {}
    }

    [NodeData("Conversions", "Int To Double")]
    [InputPin(typeof(int), 0)]
    [OutputPin(typeof(double), 0)]
    public class IntToDouble : UnsafeCastPureNode<int, double>
    {
        public IntToDouble(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base(api, nodeTransform, bounds)
        { }
    }

    [NodeData("Conversions", "Int To Bool")]
    [InputPin(typeof(int), 0)]
    [OutputPin(typeof(bool), 0)]
    public class IntToBool : UnsafeCastPureNode<int, bool>
    {
        public IntToBool(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base(api, nodeTransform, bounds)
        { }
    }

    [NodeData("Conversions", "Bool To Int")]
    [InputPin(typeof(bool), 0)]
    [OutputPin(typeof(int), 0)]
    public class BoolToInt : UnsafeCastPureNode<bool, int>
    {
        public BoolToInt(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base(api, nodeTransform, bounds)
        { }
    }

    [NodeData("Conversions", "Float To Double")]
    [InputPin(typeof(float), 0)]
    [OutputPin(typeof(double), 0)]
    public class FloatToDouble : UnsafeCastPureNode<float, double>
    {
        public FloatToDouble(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base(api, nodeTransform, bounds)
        { }
    }

    [NodeData("Conversions", "Double To Float")]
    [InputPin(typeof(double), 0)]
    [OutputPin(typeof(float), 0)]
    public class DoubleToFloat : UnsafeCastPureNode<double, float>
    {
        public DoubleToFloat(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base(api, nodeTransform, bounds)
        { }
    }
}
