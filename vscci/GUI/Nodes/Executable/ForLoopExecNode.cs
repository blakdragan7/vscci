namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes.Attributes;
    using VSCCI.GUI.Pins;

    [NodeData("Flow", "For Loop")]
    [InputPin(typeof(Exec), 0)]
    [InputPin(typeof(Number), 1)]
    [InputPin(typeof(Number), 2)]
    [OutputPin(typeof(Exec), 0)]
    [OutputPin(typeof(Number), 1)]
    [OutputPin(typeof(Exec), 2)]
    public class ForLoopExecNode : ExecutableScriptNode
    {
        public static int START_INPUT_INDEX = 1;
        public static int END_INPUT_INDEX = 2;
        public static int LOOP_OUTPUT_INDEX = 1;
        public static int LOOP_END_INDEX = 2;

        public ForLoopExecNode(ICoreClientAPI api, MatrixElementBounds bounds) : base("Forloop", "Iteration", api, bounds)
        {
            inputs.Add(new ScriptNodeInput(this, "Start Index", typeof(Number)));
            inputs.Add(new ScriptNodeInput(this, "End Index", typeof(Number)));

            outputs.Add(new ScriptNodeOutput(this, "Index", typeof(Number)));
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

        public override string GetNodeDescription()
        {
            return "This executes the \"iteration\" path exactly one time per real number between \"Start Index\" and \"End Index\". Setting \"Index\" to the current number.";
        }
    }
}
