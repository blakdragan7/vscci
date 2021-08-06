namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using System.Collections.Generic;
    using System;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes.Attributes;

    [NodeData("Constants", "Constant Number")]
    [InputPin(typeof(Number), 0)]
    [OutputPin(typeof(Number), 0)]
    class ConstantNumberScriptNode : ConstantTextInputScriptNode<Number>
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
        public ConstantNumberScriptNode(ICoreClientAPI api, MatrixElementBounds bounds) : base(api, bounds)
        {
        }

        public override bool ValidateKey(char key)
        {
            return numbers.Contains(key);
        }

        protected override Number ParseValue(string text)
        {
            try
            {
                return Number.Parse(text);
            }
            catch(Exception exc)
            {
                api.Logger.Error("Error trying to parse text {0} on Constant NumberType {1}", text, exc.Message);
            }

            return 0;
        }

        public override string GetNodeDescription()
        {
            return "This represents a constant number";
        }
    }
}
