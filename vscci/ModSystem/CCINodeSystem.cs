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
    using System;

    class CCINodeSystem : ModSystem
    {
        private ICoreServerAPI sapi;
        private ICoreClientAPI capi;

        private List<EventBasedExecutableScriptNode> eventNodes;

        private static CCINodeSystem instance = null;

        public static CCINodeSystem NodeSystem => instance;

        public CCINodeSystem() : base()
        {
            instance = this;
            eventNodes = new List<EventBasedExecutableScriptNode>();
        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.Network.RegisterChannel(Constants.NETWORK_NODE_CHANNEL)
                .RegisterMessageType<ServerNodeExecutionData>();
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
                ServerSideExecutable executable = (ServerSideExecutable)Activator.CreateInstance(Type.GetType(data.AssemblyQualifiedName));
                executable.RunServerSide(player, sapi, data.Data);
            }
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            capi = api;

        }

        public void RegisterNode(EventBasedExecutableScriptNode node)
        {
            if (capi != null)
            {
                capi.Event.EnqueueMainThreadTask(() =>
                {
                    eventNodes?.Add(node);
                }, "Node System List Add");
            }
            else
            {
                eventNodes?.Add(node);
            }
        }

        public void UnregisterNode(EventBasedExecutableScriptNode node)
        {
            if (capi != null)
            {
                capi.Event.EnqueueMainThreadTask(() =>
                {
                    eventNodes?.Remove(node);
                }, "Node System List Remove");
            }
            else
            {
                eventNodes?.Remove(node);
            }
        }

        private void OnEvent(string eventName, ref EnumHandling handling, IAttribute data)
        {
            if (capi != null)
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
            else
            {
                foreach (var node in eventNodes)
                {
                    node.OnEvent(eventName, data);
                }
            }
        }
    }
}
