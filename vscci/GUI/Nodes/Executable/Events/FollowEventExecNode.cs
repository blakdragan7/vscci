namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.Datastructures;
    using VSCCI.Data;
    using VSCCI.GUI.Nodes.Attributes;

    [NodeData("Events", "Follow Event")]
    [OutputPin(typeof(Exec), 0)]
    [OutputPin(typeof(string), 1)]
    [OutputPin(typeof(string), 2)]
    [OutputPin(typeof(string), 3)]
    class FollowEventExecNode : EventBasedExecutableScriptNode
    {
        public static int WHO_OUTPUT_INDEX = 1;
        public static int CHANNEL_OUTPUT_INDEX = 2;
        public static int PLATFORM_OUTPUT_INDEX = 3;

        private string who;
        private string channel;
        private string platform;

        public FollowEventExecNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("Follow Event", api, nodeTransform, bounds)
        {
            outputs.Add(new ScriptNodeOutput(this, "Who", typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Channel", typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Platform", typeof(string)));
        }

        protected override void OnExecute()
        {
            outputs[WHO_OUTPUT_INDEX].Value = who;
            outputs[CHANNEL_OUTPUT_INDEX].Value = channel;
            outputs[PLATFORM_OUTPUT_INDEX].Value = platform;
        }

        public override void OnEvent(string eventName, IAttribute data)
        {
            if(eventName == Constants.EVENT_FOLLOW)
            {
                var bd = data.GetValue() as FollowData;

                who = bd.who;
                channel = bd.channel;
                platform = bd.platform;

                Execute();
            }
        }
    }
}
