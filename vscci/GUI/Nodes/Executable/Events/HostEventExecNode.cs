namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.Datastructures;
    using VSCCI.Data;

    class HostEventExecNode : EventBasedExecutableScriptNode
    {
        public static int WHO_OUTPUT_INDEX = 1;
        public static int COUNT_OUTPUT_INDEX = 2;

        private int viewers;
        private string who;

        public HostEventExecNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("Host Event", api, nodeTransform, bounds)
        {
            outputs.Add(new ScriptNodeOutput(this, "Who", 1, typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Viewer Count", 1, typeof(int)));
        }

        protected override void OnExecute()
        {
            outputs[WHO_OUTPUT_INDEX].Value = who;
            outputs[COUNT_OUTPUT_INDEX].Value = viewers;
        }

        public override void OnEvent(string eventName, IAttribute data)
        {
            if(eventName == Constants.EVENT_HOST)
            {
                var bd = data.GetValue() as HostData;

                who = bd.who;
                viewers = bd.viewers;

                Execute();
            }
        }
    }
}
