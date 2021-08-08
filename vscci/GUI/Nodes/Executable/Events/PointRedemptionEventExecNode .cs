namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Datastructures;
    using VSCCI.Data;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes.Attributes;
    using VSCCI.GUI.Pins;

    [NodeData("Events", "Redemption Event")]
    [OutputPin(typeof(Exec), 0)]
    [OutputPin(typeof(string), 1)]
    [OutputPin(typeof(string), 2)]
    [OutputPin(typeof(string), 3)]
    [OutputPin(typeof(string), 4)]
    class PointRedemptionEventExecNode : EventBasedExecutableScriptNode
    {
        public static int WHO_OUTPUT_INDEX = 1;
        public static int NAME_OUTPUT_INDEX = 2;
        public static int ID_OUTPUT_INDEX = 3;
        public static int MESSAGE_OUTPUT_INDEX = 4;

        private string name;
        private string id;
        private string message;
        private string who;

        public PointRedemptionEventExecNode(ICoreClientAPI api, MatrixElementBounds bounds) : base("Point Redemption Event", api, bounds)
        {
            outputs.Add(new ScriptNodeOutput(this, "Who", typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Name", typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Id", typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Message", typeof(string)));
        }

        protected override void OnExecute()
        {
            outputs[WHO_OUTPUT_INDEX].Value = who;
            outputs[NAME_OUTPUT_INDEX].Value = name;
            outputs[ID_OUTPUT_INDEX].Value = id;
            outputs[MESSAGE_OUTPUT_INDEX].Value = message;
        }

        public override void OnEvent(string eventName, IAttribute data)
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
