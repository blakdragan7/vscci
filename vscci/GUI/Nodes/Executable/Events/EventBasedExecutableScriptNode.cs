namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.Datastructures;

    public abstract class EventBasedExecutableScriptNode : ExecutableScriptNode
    {
        public EventBasedExecutableScriptNode(string title, ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base(title, api, nodeTransform, bounds, true, false)
        {
            api.Event.RegisterEventBusListener(OnEvent);
        }

        protected abstract void OnEvent(string eventName, ref EnumHandling handling, IAttribute data);
    }
}
