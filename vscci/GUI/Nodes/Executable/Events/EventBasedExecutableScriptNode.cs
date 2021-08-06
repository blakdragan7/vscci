namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Elements;
    using Vintagestory.API.Datastructures;

    using VSCCI.ModSystem;

    public abstract class EventBasedExecutableScriptNode : ExecutableScriptNode
    {
        public EventBasedExecutableScriptNode(string title, ICoreClientAPI api, MatrixElementBounds bounds) : base(title, api, bounds, true, false)
        {
            CCINodeSystem.NodeSystem.RegisterNode(this);
        }

        public override void Dispose()
        {
            base.Dispose();

            CCINodeSystem.NodeSystem.UnregisterNode(this);
        }

        public abstract void OnEvent(string eventName, IAttribute data);

        public override string GetNodeDescription()
        {
            return "This represents an event that came from some form of Streaming service, such as twitch, youtube or streamlabs.";
        }
    }
}
