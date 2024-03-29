﻿namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.Datastructures;
    using VSCCI.Data;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes.Attributes;
    using VSCCI.GUI.Pins;

    [NodeData("Events", "Host Event")]
    [OutputPin(typeof(Exec), 0)]
    [OutputPin(typeof(string), 1)]
    [OutputPin(typeof(Number), 2)]
    class HostEventExecNode : EventBasedExecutableScriptNode
    {
        public static int WHO_OUTPUT_INDEX = 1;
        public static int COUNT_OUTPUT_INDEX = 2;

        private int viewers;
        private string who;

        public HostEventExecNode(ICoreClientAPI api, MatrixElementBounds bounds) : base("Host Event", api, bounds)
        {
            outputs.Add(new ScriptNodeOutput(this, "Who", typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Viewer Count", typeof(Number)));
        }

        protected override void OnExecute()
        {
            outputs[WHO_OUTPUT_INDEX].Value = who;
            outputs[COUNT_OUTPUT_INDEX].Value = viewers;
        }

        public override void OnEvent(string eventName, IAttribute data)
        {
            if(eventName == Constants.EVENT_HOST)
            {
                var bd = data.GetValue() as HostData;

                who = bd.who;
                viewers = bd.viewers;

                Execute();
            }
        }
    }
}
