namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.Datastructures;

    using VSCCI.ModSystem;

    public abstract class EventBasedExecutableScriptNode : ExecutableScriptNode
    {
        public EventBasedExecutableScriptNode(string title, ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base(title, api, nodeTransform, bounds, true, false)
        {
            CCINodeSystem.NodeSystem.RegisterNode(this);
        }

        public override void Dispose()
        {
            base.Dispose();

            CCINodeSystem.NodeSystem.UnregisterNode(this);
        }

        public abstract void OnEvent(string eventName, IAttribute data);
    }
}
