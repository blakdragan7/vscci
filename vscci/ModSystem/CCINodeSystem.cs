namespace VSCCI.ModSystem
{
    using Vintagestory.API.Common;
    using Vintagestory.API.Server;
    using Vintagestory.API.Client;
    using Vintagestory.API.Datastructures;

    using VSCCI.Data;
    using VSCCI.GUI.Nodes;
    using System.Collections.Generic;
    using System.Threading;

    class CCINodeSystem : ModSystem
    {
        private ICoreServerAPI sapi;
        private ICoreClientAPI capi;

        private List<EventBasedExecutableScriptNode> eventNodes;

        private static CCINodeSystem instance = null;

        public static CCINodeSystem NodeSystem => instance;

        public override void Start(ICoreAPI api)
        {
            instance = this;
            eventNodes = new List<EventBasedExecutableScriptNode>();

            base.Start(api);
            api.Network.RegisterChannel(Constants.NETWORK_NODE_CHANNEL);
            api.Event.RegisterEventBusListener(OnEvent);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            sapi = api;
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);

            capi = api;
        }

        public void RegisterNode(EventBasedExecutableScriptNode node)
        {
           capi.Event.EnqueueMainThreadTask(() =>
           {
               eventNodes.Add(node);
           }, "Node System List Add");
        }

        public void UnregisterNode(EventBasedExecutableScriptNode node)
        {
            capi.Event.EnqueueMainThreadTask(() =>
            {
                eventNodes.Remove(node);
            }, "Node System List Remove");
        }

        private void OnEvent(string eventName, ref EnumHandling handling, IAttribute data)
        {
            // TODO: make a better way for this to be thread safe
            capi.Event.EnqueueMainThreadTask(() =>
            {
                foreach (var node in eventNodes)
                {
                    node.OnEvent(eventName, data);
                }
            }, "Node System On Event");
        }
    }
}
