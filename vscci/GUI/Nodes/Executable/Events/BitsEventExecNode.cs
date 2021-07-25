﻿namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.Datastructures;
    using VSCCI.Data;

    class BitsEventExecNode : EventBasedExecutableScriptNode
    {
        public static int AMOUNT_OUTPUT_INDEX = 1;
        public static int FROM_OUTPUT_INDEX = 2;
        public static int MESSAGE_OUTPUT_INDEX = 3;

        private int amount;
        private string message;
        private string from;

        public BitsEventExecNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("Bit Event", api, nodeTransform, bounds)
        {
            outputs.Add(new ScriptNodeOutput(this, "amount", 1, typeof(int)));
            outputs.Add(new ScriptNodeOutput(this, "from", 1, typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "message", 1, typeof(string)));
        }

        protected override void OnExecute()
        {
            outputs[AMOUNT_OUTPUT_INDEX].Value = amount;
            outputs[FROM_OUTPUT_INDEX].Value = from;
            outputs[MESSAGE_OUTPUT_INDEX].Value = message;
        }

        protected override void OnEvent(string eventName, ref EnumHandling handling, IAttribute data)
        {
            if(eventName == Constants.EVENT_BITS_RECIEVED)
            {
                var bd = data.GetValue() as BitsData;

                amount = bd.amount;
                from = bd.from;
                message = bd.message;

                Execute();
            }
        }
    }
}
