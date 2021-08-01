namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.Datastructures;
    using VSCCI.Data;
    using VSCCI.GUI.Nodes.Attributes;

    [NodeData("Events", "Sub Event")]
    [OutputPin(typeof(Exec), 0)]
    [OutputPin(typeof(string), 1)]
    [OutputPin(typeof(string), 2)]
    [OutputPin(typeof(string), 3)]
    [OutputPin(typeof(bool), 4)]
    class SubEventExecNode : EventBasedExecutableScriptNode
    {
        public static int FROM_OUTPUT_INDEX = 1;
        public static int TO_OUTPUT_INDEX = 2;
        public static int MESSAGE_OUTPUT_INDEX = 3;
        public static int ISGIFT_OUTPUT_INDEX = 4;

        private string message;
        private string from;
        private string to;
        private bool isGift;

        public SubEventExecNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("Sub Event", api, nodeTransform, bounds)
        {
            outputs.Add(new ScriptNodeOutput(this, "From", typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "To", typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Message", typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "IsGift", typeof(bool)));
        }

        protected override void OnExecute()
        {
            outputs[TO_OUTPUT_INDEX].Value = to;
            outputs[FROM_OUTPUT_INDEX].Value = from;
            outputs[MESSAGE_OUTPUT_INDEX].Value = message;
            outputs[ISGIFT_OUTPUT_INDEX].Value = isGift;
        }

        public override void OnEvent(string eventName, IAttribute data)
        {
            if(eventName == Constants.EVENT_NEW_SUB)
            {
                var bd = data.GetValue() as NewSubData;

                from = bd.from;
                message = bd.message;
                isGift = bd.isGift;
                to = bd.to;

                Execute();
            }
        }
    }
}
