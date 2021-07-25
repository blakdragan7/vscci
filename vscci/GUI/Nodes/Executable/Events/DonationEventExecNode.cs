namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.Datastructures;
    using VSCCI.Data;

    class DonationEventExecNode : EventBasedExecutableScriptNode
    {
        public static int WHO_OUTPUT_INDEX = 1;
        public static int AMOUNT_OUTPUT_INDEX = 2;
        public static int MESSAGE_OUTPUT_INDEX = 3;

        private string who;
        private float amount;
        private string message;

        public DonationEventExecNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("Donation Event", api, nodeTransform, bounds)
        {
            outputs.Add(new ScriptNodeOutput(this, "Who", 1, typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Amount", 1, typeof(int)));
            outputs.Add(new ScriptNodeOutput(this, "Message", 1, typeof(string)));
        }

        protected override void OnExecute()
        {
            outputs[AMOUNT_OUTPUT_INDEX].Value = amount;
            outputs[WHO_OUTPUT_INDEX].Value = who;
            outputs[MESSAGE_OUTPUT_INDEX].Value = message;
        }

        protected override void OnEvent(string eventName, ref EnumHandling handling, IAttribute data)
        {
            if(eventName == Constants.EVENT_DONATION)
            {
                var bd = data.GetValue() as DonationData;

                amount = bd.amount;
                who = bd.who;
                message = bd.message;

                Execute();
            }
        }
    }
}
