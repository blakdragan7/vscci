namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.Datastructures;
    using VSCCI.Data;

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
            outputs.Add(new ScriptNodeOutput(this, "Who", 1, typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Channel", 1, typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Platform", 1, typeof(string)));
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
