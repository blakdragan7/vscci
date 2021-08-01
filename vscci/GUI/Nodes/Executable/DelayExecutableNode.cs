namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Nodes.Attributes;

    [NodeData("Flow", "Delay")]
    [InputPin(typeof(Exec), 0)]
    [InputPin(typeof(NumberType), 1)]
    [OutputPin(typeof(Exec), 0)]
    class DelayExecutableNode : ExecutableScriptNode
    {
        public static int MILISECONDS_INPUT_INDEX = 1;
        public DelayExecutableNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("Delay", api, nodeTransform, bounds)
        {
            inputs.Add(new ScriptNodeInput(this, "miliSeconds", typeof(NumberType)));
        }

        protected override void OnExecute()
        {
            int miliSeconds = (int)inputs[MILISECONDS_INPUT_INDEX].GetInput();
            System.Threading.Thread.Sleep(miliSeconds);
        }
    }
}
