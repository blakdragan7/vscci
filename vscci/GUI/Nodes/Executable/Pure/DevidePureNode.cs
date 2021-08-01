namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;

    public class DevidePureNode<T> : ExecutableScriptNode
    {
        public static int INPUT_ONE_INDEX = 0;
        public static int INPUT_TWO_INDEX = 1;
        public static int OUTPUT_INDEX = 0;
        public DevidePureNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("/", api, nodeTransform, bounds, true)
        {
            inputs.Add(new ScriptNodeInput(this, "Top", typeof(T)));
            inputs.Add(new ScriptNodeInput(this, "Bottom", typeof(T)));

            outputs.Add(new ScriptNodeOutput(this, "Result", typeof(T)));

            shouldAutoExecuteNext = false;
        }

        protected override void OnExecute()
        {
            dynamic first = inputs[INPUT_ONE_INDEX].GetInput();
            dynamic second = inputs[INPUT_TWO_INDEX].GetInput();

            outputs[OUTPUT_INDEX].Value = first / second;
        }
    }
}
