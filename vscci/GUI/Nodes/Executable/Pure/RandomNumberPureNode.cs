namespace VSCCI.GUI.Nodes
{
    using Vintagestory.API.Client;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes.Attributes;
    using VSCCI.GUI.Pins;

    [NodeData("Util", "Random Number")]
    [InputPin(typeof(Number), 0)]
    [InputPin(typeof(Number), 1)]
    [OutputPin(typeof(Number), 0)]
    public class RandomNumberPureNode : ExecutableScriptNode
    {
        private readonly System.Random random = new System.Random();

        public RandomNumberPureNode(ICoreClientAPI api, MatrixElementBounds bounds) : base("Random", api, bounds, true)
        {
            inputs.Add(new ScriptNodeNumberInput(this, "Min"));
            inputs.Add(new ScriptNodeNumberInput(this, "Max"));
            outputs.Add(new ScriptNodeOutput(this, "Result", typeof(Number)));
        }

        protected override void OnExecute()
        {
            Number min = inputs[0].GetInput();
            Number max = inputs[1].GetInput();

            Number result = random.Next(min, max);

            outputs[0].Value = result;
        }

        public override string GetNodeDescription()
        {
            return "Returns a random number between \"Min\" and \"Max\" Inclusive";
        }
    }
}
