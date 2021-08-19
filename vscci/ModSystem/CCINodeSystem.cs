namespace VSCCI.ModSystem
{
    using Vintagestory.API.Common;
    using Vintagestory.API.Server;
    using Vintagestory.API.Client;
    using Vintagestory.API.Datastructures;

    using VSCCI.Data;
    using VSCCI.GUI.Nodes;

    using System;
    using System.Threading;
    using System.Collections.Generic;

    class CCINodeSystem : ModSystem
    {
        private ICoreServerAPI sapi;
        private ICoreClientAPI capi;

        private readonly object eventLockObject = new object();
        private readonly object serverMessageLockObject = new object();

        private List<EventBasedExecutableScriptNode> eventNodes;
        private Dictionary<Guid, ServerSideExecutableNode> serverNodes;

        private static CCINodeSystem instance = null;

        public static CCINodeSystem NodeSystem => instance;

        public CCINodeSystem() : base()
        {
            instance = this;
            eventNodes = new List<EventBasedExecutableScriptNode>();
            serverNodes = new Dictionary<Guid, ServerSideExecutableNode>();
        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.Network.RegisterChannel(Constants.NETWORK_NODE_CHANNEL)
                .RegisterMessageType<ServerNodeExecutionData>()
                .RegisterMessageType<PlayerPositionData>();
            api.Event.RegisterEventBusListener(OnEvent);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            sapi = api;
            api.Network.GetChannel(Constants.NETWORK_NODE_CHANNEL)
                .SetMessageHandler<ServerNodeExecutionData>(ExecuteNodeServerSide);
        }

        private void ExecuteNodeServerSide(IServerPlayer player, ServerNodeExecutionData data)
        {
            if (ConfigData.PlayerIsAllowed(player))
            {
                ServerSideAction executable = (ServerSideAction)Activator.CreateInstance(Type.GetType(data.AssemblyQualifiedName));
                executable.RunServerSide(player, sapi, data);
            }
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            capi = api;

            api.Network.GetChannel(Constants.NETWORK_NODE_CHANNEL)
                .SetMessageHandler<PlayerPositionData>(ReceivedPlayerPosition);
        }

        public void RegisterNodeForEvents(EventBasedExecutableScriptNode node)
        {
            lock (eventLockObject)
            {
                eventNodes?.Add(node);
            }
        }

        public void UnregisterNodeForEvents(EventBasedExecutableScriptNode node)
        {
            lock (eventLockObject)
            {
                eventNodes?.Remove(node);
            }
        }

        public void RegisterNodeForServerMessages(ServerSideExecutableNode node)
        {
            lock (serverMessageLockObject)
            {
                serverNodes.Add(node.Guid, node);
            }
        }

        public void UnregisterNodeForServerMessages(ServerSideExecutableNode node)
        {
            lock (serverMessageLockObject)
            {
                serverNodes.Remove(node.Guid);
            }
        }

        private void ReceivedPlayerPosition(PlayerPositionData data)
        {
            lock (serverMessageLockObject)
            {
                ServerSideExecutableNode node = null;
                if (serverNodes.TryGetValue(data.Guid, out node))
                {
                    node.ReceivedServerMessage(data);
                }
            }
        }

        private void OnEvent(string eventName, ref EnumHandling handling, IAttribute data)
        {
            lock (eventLockObject)
            {
                foreach (var node in eventNodes)
                {
                    node.OnEvent(eventName, data);
                }
            };
        }
    }
}
