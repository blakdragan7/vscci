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

    [NodeData("Events", "Super Chat Event")]
    [OutputPin(typeof(Exec), 0)]
    [OutputPin(typeof(string), 1)]
    [OutputPin(typeof(string), 2)]
    [OutputPin(typeof(Number), 3)]

    class SuperChatEventExecNode : EventBasedExecutableScriptNode
    {
        public static int FROM_OUTPUT_INDEX = 1;
        public static int MESSAGE_OUTPUT_INDEX = 2;
        public static int AMOUNT_OUTPUT_INDEX = 3;

        private float amount;
        private string message;
        private string from;

        public SuperChatEventExecNode(ICoreClientAPI api, MatrixElementBounds bounds) : base("Super Chat Event", api, bounds)
        {
            outputs.Add(new ScriptNodeOutput(this, "From", typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Message", typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Amount", typeof(Number)));
        }

        protected override void OnExecute()
        {
            outputs[AMOUNT_OUTPUT_INDEX].Value = amount;
            outputs[FROM_OUTPUT_INDEX].Value = from;
            outputs[MESSAGE_OUTPUT_INDEX].Value = message;
        }

        public override void OnEvent(string eventName, IAttribute data)
        {
            if(eventName == Constants.EVENT_SCHAT)
            {
                var bd = data.GetValue() as SuperChatData;

                amount = bd.amount;
                from = bd.who;
                message = bd.comment;

                Execute();
            }
        }
    }
}
