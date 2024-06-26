﻿namespace VSCCI.GUI.Nodes
{
    using Vintagestory.API.Client;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes.Attributes;
    using VSCCI.GUI.Pins;

    [NodeData("Basic", "Append String")]
    [InputPin(typeof(string), 0)]
    [InputPin(typeof(string), 1)]
    [OutputPin(typeof(string), 0)]
    public class AppenStringPureNode : ExecutableScriptNode
    {
        public static int INPUT_ONE_INDEX = 0;
        public static int INPUT_TWO_INDEX = 1;
        public static int OUTPUT_INDEX = 0;
        public AppenStringPureNode(ICoreClientAPI api, MatrixElementBounds bounds) : base("Append String", api, bounds, true)
        {
            inputs.Add(new ScriptNodeInput(this, "First", typeof(string)));
            inputs.Add(new ScriptNodeInput(this, "Second", typeof(string)));

            outputs.Add(new ScriptNodeOutput(this, "Result", typeof(string)));

            shouldAutoExecuteNext = false;
        }

        protected override void OnExecute()
        {
            dynamic firstInput = inputs[INPUT_ONE_INDEX].GetInput();
            dynamic secondInput = inputs[INPUT_TWO_INDEX].GetInput();
            string first = firstInput is string ? firstInput : firstInput.ToString();
            string second = secondInput is string ? secondInput : secondInput.ToString();

            outputs[OUTPUT_INDEX].Value = first + second;
        }

        public override string GetNodeDescription()
        {
            return "This will add \"Second\" to the end of \"First\"";
        }
    }
}
