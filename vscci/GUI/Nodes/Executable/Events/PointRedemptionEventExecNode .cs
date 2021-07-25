namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.Datastructures;
    using VSCCI.Data;

    class PointRedemptionEventExecNode : EventBasedExecutableScriptNode
    {
        public static int WHO_OUTPUT_INDEX = 1;
        public static int NAME_OUTPUT_INDEX = 2;
        public static int ID_OUTPUT_INDEX = 3;
        public static int MESSAGE_OUTPUT_INDEX = 3;

        private string name;
        private string id;
        private string message;
        private string who;

        public PointRedemptionEventExecNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("Point Redemption Event", api, nodeTransform, bounds)
        {
            outputs.Add(new ScriptNodeOutput(this, "Who", 1, typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Name", 1, typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Id", 1, typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Message", 1, typeof(string)));
        }

        protected override void OnExecute()
        {
            outputs[WHO_OUTPUT_INDEX].Value = who;
            outputs[NAME_OUTPUT_INDEX].Value = name;
            outputs[ID_OUTPUT_INDEX].Value = id;
            outputs[MESSAGE_OUTPUT_INDEX].Value = message;
        }

        protected override void OnEvent(string eventName, ref EnumHandling handling, IAttribute data)
        {
            if(eventName == Constants.EVENT_REDEMPTION)
            {
                var bd = data.GetValue() as PointRedemptionData;

                who = bd.who;
                name = bd.redemptionName;
                id = bd.redemptionID;
                message = bd.message;

                Execute();
            }
        }
    }
}
