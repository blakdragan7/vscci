namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Nodes.Attributes;

    [NodeData("Flow", "For Loop")]
    [InputPin(typeof(Exec), 0)]
    [InputPin(typeof(int), 1)]
    [InputPin(typeof(int), 2)]
    [OutputPin(typeof(Exec), 0)]
    [OutputPin(typeof(int), 1)]
    [OutputPin(typeof(Exec), 2)]
    public class ForLoopExecNode : ExecutableScriptNode
    {
        public static int START_INPUT_INDEX = 1;
        public static int END_INPUT_INDEX = 2;
        public static int LOOP_OUTPUT_INDEX = 1;
        public static int LOOP_END_INDEX = 2;

        public ForLoopExecNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("Forloop", "Iteration", api, nodeTransform, bounds)
        {
            inputs.Add(new ScriptNodeInput(this, "Start Index", typeof(int)));
            inputs.Add(new ScriptNodeInput(this, "End Index", typeof(int)));

            outputs.Add(new ScriptNodeOutput(this, "Index", typeof(int)));
            outputs.Add(new ExecOutputNode(this, "Done"));
        }

        protected override void OnExecute()
        {
            int start = inputs[START_INPUT_INDEX].GetInput();
            int end = inputs[END_INPUT_INDEX].GetInput();

            for(var i=start;i<end;i++)
            {
                outputs[LOOP_OUTPUT_INDEX].Value = i;

                ExecuteNextNode();
            }

            nextExecutableIndex = LOOP_END_INDEX;
        }
    }
}
