namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes.Attributes;
    using VSCCI.GUI.Pins;

    [NodeData("Flow", "Delay")]
    [InputPin(typeof(Exec), 0)]
    [InputPin(typeof(Number), 1)]
    [OutputPin(typeof(Exec), 0)]
    class DelayExecutableNode : ExecutableScriptNode
    {
        public static int MILISECONDS_INPUT_INDEX = 1;
        public DelayExecutableNode(ICoreClientAPI api, MatrixElementBounds bounds) : base("Delay", api, bounds)
        {
            inputs.Add(new ScriptNodeNumberInput(this, "miliSeconds"));
        }

        protected override void OnExecute()
        {
            int miliSeconds = (int)inputs[MILISECONDS_INPUT_INDEX].GetInput();
            System.Threading.Thread.Sleep(miliSeconds);
        }

        public override string GetNodeDescription()
        {
            return "This Delays the execution of the next connected node by \"miliSeconds\" which is 1/1000 of a second.";
        }
    }
}
