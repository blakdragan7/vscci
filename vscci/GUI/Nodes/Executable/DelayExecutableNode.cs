﻿namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Nodes.Attributes;

    [NodeData("Flow", "Delay")]
    [InputPin(typeof(Exec), 0)]
    [InputPin(typeof(Number), 1)]
    [OutputPin(typeof(Exec), 0)]
    class DelayExecutableNode : ExecutableScriptNode
    {
        public static int MILISECONDS_INPUT_INDEX = 1;
        public DelayExecutableNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("Delay", api, nodeTransform, bounds)
        {
            inputs.Add(new ScriptNodeInput(this, "miliSeconds", typeof(Number)));
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
