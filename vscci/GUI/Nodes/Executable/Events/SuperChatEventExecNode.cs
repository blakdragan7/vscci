namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.Datastructures;
    using VSCCI.Data;

    class SuperChatEventExecNode : EventBasedExecutableScriptNode
    {
        public static int FROM_OUTPUT_INDEX = 1;
        public static int MESSAGE_OUTPUT_INDEX = 2;
        public static int AMOUNT_OUTPUT_INDEX = 3;

        private float amount;
        private string message;
        private string from;

        public SuperChatEventExecNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("Super Chat Event", api, nodeTransform, bounds)
        {
            outputs.Add(new ScriptNodeOutput(this, "From", 1, typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Message", 1, typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Amount", 1, typeof(float)));
        }

        protected override void OnExecute()
        {
            outputs[AMOUNT_OUTPUT_INDEX].Value = amount;
            outputs[FROM_OUTPUT_INDEX].Value = from;
            outputs[MESSAGE_OUTPUT_INDEX].Value = message;
        }

        protected override void OnEvent(string eventName, ref EnumHandling handling, IAttribute data)
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
