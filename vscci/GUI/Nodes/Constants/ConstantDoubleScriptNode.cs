namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using System.Collections.Generic;
    using System;
    using VSCCI.GUI.Nodes.Attributes;

    [NodeData("Constants", "Constant Double")]
    [InputPin(typeof(double), 0)]
    [OutputPin(typeof(double), 0)]
    class ConstantDoubleScriptNode : ConstantTextInputScriptNode<double>
    {
        private static List<char> numbers = new List<char>()
        {
            '0',
            '1',
            '2',
            '3',
            '4',
            '5',
            '6',
            '7',
            '8',
            '9',
            '.'
        };
        public ConstantDoubleScriptNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base(api, nodeTransform, bounds)
        {
        }

        public override bool ValidateKey(char key)
        {
            return numbers.Contains(key);
        }

        protected override double ParseValue(string text)
        {
            try
            {
                return double.Parse(text);
            }
            catch(Exception exc)
            {
                api.Logger.Error("Error trying to parse value on Constant Double {0}", exc.Message);
            }

            return 0;
        }
    }
}
