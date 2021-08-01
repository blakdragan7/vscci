namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using System.Collections.Generic;
    using System;
    using VSCCI.GUI.Nodes.Attributes;

    [NodeData("Constants", "Constant Number")]
    [InputPin(typeof(NumberType), 0)]
    [OutputPin(typeof(NumberType), 0)]
    class ConstantNumberScriptNode : ConstantTextInputScriptNode<NumberType>
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
        public ConstantNumberScriptNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base(api, nodeTransform, bounds)
        {
        }

        public override bool ValidateKey(char key)
        {
            return numbers.Contains(key);
        }

        protected override NumberType ParseValue(string text)
        {
            try
            {
                return NumberType.Parse(text);
            }
            catch(Exception exc)
            {
                api.Logger.Error("Error trying to parse text {0} on Constant NumberType {1}", text, exc.Message);
            }

            return 0;
        }
    }
}
