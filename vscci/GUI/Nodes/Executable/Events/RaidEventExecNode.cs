namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.Datastructures;
    using VSCCI.Data;
    using VSCCI.GUI.Nodes.Attributes;

    [NodeData("Events", "Raid Event")]
    [OutputPin(typeof(Exec), 0)]
    [InputPin(typeof(string), 1)]
    [InputPin(typeof(int), 2)]
    class RaidEventExecNode : EventBasedExecutableScriptNode
    {
        public static int CHANNEL_OUTPUT_INDEX = 1;
        public static int VIEWER_OUTPUT_INDEX = 2;

        private int viewerCount;
        private string channel;

        public RaidEventExecNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("Raid Event", api, nodeTransform, bounds)
        {
            outputs.Add(new ScriptNodeOutput(this, "Channel", typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Viewer Count", typeof(int)));
        }

        protected override void OnExecute()
        {
            outputs[CHANNEL_OUTPUT_INDEX].Value = channel;
            outputs[VIEWER_OUTPUT_INDEX].Value = viewerCount;
        }

        public override void OnEvent(string eventName, IAttribute data)
        {
            if(eventName == Constants.EVENT_RAID)
            {
                var bd = data.GetValue() as RaidData;

                channel = bd.raidChannel;
                viewerCount = bd.numberOfViewers;

                Execute();
            }
        }
    }
}
