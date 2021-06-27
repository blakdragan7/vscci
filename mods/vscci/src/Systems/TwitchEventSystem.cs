using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;
using vscci.src.Data;

using vscci.src.CCIIntegrations.Twitch;

namespace vscci.src.Systems
{
    class TwitchEventSystem : ModSystem
    {
        private ICoreClientAPI capi;
        private ICoreServerAPI sapi;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.Network.RegisterChannel(Constants.NETWORK_CHANNEL)
                .RegisterMessageType(typeof(TwitchRaidData))
                .RegisterMessageType(typeof(TwitchBitsData))
                .RegisterMessageType(typeof(TwitchFollowData))
                .RegisterMessageType(typeof(TwitchNewSubData))
                .RegisterMessageType(typeof(TwitchPointRedemptionData));
        }

        #region Server
        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            sapi = api;
            api.Event.RegisterEventBusListener(OnServerEvent);
        }

        private void OnServerEvent(string eventName, ref EnumHandling handling, IAttribute data)
        {
            switch (eventName)
            {
                case Constants.TWITCH_EVENT_BITS_RECIEVED:
                    break;
                case Constants.TWITCH_EVENT_REDEMPTION:
                    break;
                case Constants.TWITCH_EVENT_NEW_SUB:
                    break;
                case Constants.TWITCH_EVENT_FOLLOW:
                    break;
                case Constants.TWITCH_EVENT_RAID:
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Client
        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            capi = api;

            api.Network.GetChannel(Constants.NETWORK_CHANNEL)
                .SetMessageHandler<TwitchRaidData>(OnTwitchRaidMessage)
                .SetMessageHandler<TwitchBitsData>(OnTwitchBitsMessage)
                .SetMessageHandler<TwitchFollowData>(OnTwitchFollowMessage)
                .SetMessageHandler<TwitchNewSubData>(OnTwitchNewSubMessage)
                .SetMessageHandler<TwitchPointRedemptionData>(OnTwitchPointRedemptionMessage);
        }

        private void OnTwitchRaidMessage(TwitchRaidData @event)
        {
            capi.Event.PushEvent(Constants.TWITCH_EVENT_RAID, new ByteArrayAttribute(SerializerUtil.Serialize(@event)));
        }

        private void OnTwitchBitsMessage(TwitchBitsData @event)
        {
            capi.Event.PushEvent(Constants.TWITCH_EVENT_BITS_RECIEVED, new ByteArrayAttribute(SerializerUtil.Serialize(@event)));
        }

        private void OnTwitchFollowMessage(TwitchFollowData @event)
        {
            capi.Event.PushEvent(Constants.TWITCH_EVENT_FOLLOW, new ByteArrayAttribute(SerializerUtil.Serialize(@event)));
        }

        private void OnTwitchNewSubMessage(TwitchNewSubData @event)
        {
            capi.Event.PushEvent(Constants.TWITCH_EVENT_NEW_SUB, new ByteArrayAttribute(SerializerUtil.Serialize(@event)));
        }

        private void OnTwitchPointRedemptionMessage(TwitchPointRedemptionData @event)
        {
            capi.Event.PushEvent(Constants.TWITCH_EVENT_REDEMPTION, new ByteArrayAttribute(SerializerUtil.Serialize(@event)));
        }

        #endregion
    }
}
