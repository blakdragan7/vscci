namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;

    class DelayExecutableNode : ExecutableScriptNode
    {
        public static int MILISECONDS_INPUT_INDEX = 1;
        public DelayExecutableNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("Delay", api, nodeTransform, bounds)
        {
            inputs.Add(new ScriptNodeInput(this, "miliSeconds", typeof(int)));
        }

        protected override void OnExecute()
        {
            int miliSeconds = inputs[MILISECONDS_INPUT_INDEX].GetInput();
            System.Threading.Thread.Sleep(miliSeconds);
        }
    }
}
