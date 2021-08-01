namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Nodes.Attributes;

    [NodeData("Flow", "For Loop")]
    [InputPin(typeof(Exec), 0)]
    [InputPin(typeof(NumberType), 1)]
    [InputPin(typeof(NumberType), 2)]
    [OutputPin(typeof(Exec), 0)]
    [OutputPin(typeof(NumberType), 1)]
    [OutputPin(typeof(Exec), 2)]
    public class ForLoopExecNode : ExecutableScriptNode
    {
        public static int START_INPUT_INDEX = 1;
        public static int END_INPUT_INDEX = 2;
        public static int LOOP_OUTPUT_INDEX = 1;
        public static int LOOP_END_INDEX = 2;

        public ForLoopExecNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("Forloop", "Iteration", api, nodeTransform, bounds)
        {
            inputs.Add(new ScriptNodeInput(this, "Start Index", typeof(NumberType)));
            inputs.Add(new ScriptNodeInput(this, "End Index", typeof(NumberType)));

            outputs.Add(new ScriptNodeOutput(this, "Index", typeof(NumberType)));
            outputs.Add(new ExecOutputNode(this, "Done"));
        }

        protected override void OnExecute()
        {
            int start = (int)inputs[START_INPUT_INDEX].GetInput();
            int end = (int)inputs[END_INPUT_INDEX].GetInput();

            for(var i=start;i<end;i++)
            {
                outputs[LOOP_OUTPUT_INDEX].Value = i;

                ExecuteNextNode();
            }

            nextExecutableIndex = LOOP_END_INDEX;
        }
    }
}
