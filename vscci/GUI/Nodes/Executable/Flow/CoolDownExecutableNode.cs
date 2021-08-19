namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes.Attributes;
    using VSCCI.GUI.Pins;

    [NodeData("Flow", "Cool Down")]
    [InputPin(typeof(Exec), 0)]
    [InputPin(typeof(Number), 1)]
    [OutputPin(typeof(Exec), 0)]
    class CoolDownExecutableNode : ExecutableScriptNode
    {
        public static int MILISECONDS_INPUT_INDEX = 1;
        private System.DateTime previousExecutionTime;
        private bool hasDateTime;

        public CoolDownExecutableNode(ICoreClientAPI api, MatrixElementBounds bounds) : base("Cool Down", api, bounds)
        {
            hasDateTime = false;
            inputs.Add(new ScriptNodeNumberInput(this, "MiliSeconds"));
        }

        protected override void OnExecute()
        {
            Number miliSeconds = (int)inputs[MILISECONDS_INPUT_INDEX].GetInput();
            System.DateTime currentDateTime = System.DateTime.UtcNow;

            shouldAutoExecuteNext = ((currentDateTime - previousExecutionTime) >= System.TimeSpan.FromMilliseconds(miliSeconds))
                                    || hasDateTime == false;

            if(shouldAutoExecuteNext)
                previousExecutionTime = currentDateTime;

            hasDateTime = true;
        }

        public override string GetNodeDescription()
        {
            return "Executes \"Exec\" only once per \"Time\".";
        }
    }
}
