namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using System.Collections.Generic;
    using System;

    class ConstantIntScriptNode : ConstantTextInputScriptNode<int>
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
        };
        public ConstantIntScriptNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base(api, nodeTransform, bounds)
        {
        }

        public override bool ValidateKey(char key)
        {
            return numbers.Contains(key);
        }

        protected override int ParseValue(string text)
        {
            try
            {
                return int.Parse(text);
            }
            catch(Exception exc)
            {
                api.Logger.Error("Error trying to parse value on Constant Int", exc.Message);
            }

            return 0;
        }
    }
}
