namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using System.Collections.Generic;
    using System;
    using VSCCI.GUI.Nodes.Attributes;

    [NodeData("Constants", "Constant Float")]
    [InputPin(typeof(float), 0)]
    [OutputPin(typeof(float), 0)]
    class ConstantFloatScriptNode : ConstantTextInputScriptNode<float>
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
        public ConstantFloatScriptNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base(api, nodeTransform, bounds)
        {
        }

        public override bool ValidateKey(char key)
        {
            return numbers.Contains(key);
        }

        protected override float ParseValue(string text)
        {
            try
            {
                return float.Parse(text);
            }
            catch(Exception exc)
            {
                api.Logger.Error("Error trying to parse value on Constant Float {0}", exc.Message);
            }

            return 0;
        }
    }
}
