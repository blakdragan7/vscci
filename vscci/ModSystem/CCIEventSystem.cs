namespace vscci.ModSystem
{
    using Vintagestory.API.Client;
    using Vintagestory.API.Server;
    using Vintagestory.API.Common;
    using Vintagestory.API.Datastructures;

    using vscci.Data;
    using vscci.CCIIntegrations.Twitch;
    public class CCIEventSystem : ModSystem
    {
        private ICoreClientAPI capi;
        private ICoreServerAPI sapi;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.Network.RegisterChannel(Constants.NETWORK_EVENT_CHANNEL)
                .RegisterMessageType(typeof(RaidData))
                .RegisterMessageType(typeof(BitsData))
                .RegisterMessageType(typeof(FollowData))
                .RegisterMessageType(typeof(NewSubData))
                .RegisterMessageType(typeof(PointRedemptionData));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            sapi = api;

            api.Network.GetChannel(Constants.NETWORK_EVENT_CHANNEL)
                .SetMessageHandler<RaidData>(OnTwitchRaidMessage)
                .SetMessageHandler<BitsData>(OnTwitchBitsMessage)
                .SetMessageHandler<FollowData>(OnTwitchFollowMessage)
                .SetMessageHandler<NewSubData>(OnTwitchNewSubMessage)
                .SetMessageHandler<PointRedemptionData>(OnTwitchPointRedemptionMessage);
        }

        private void OnTwitchRaidMessage(IServerPlayer player, RaidData @event)
        {
            if (ConfigData.PlayerIsAllowed(player))
            {
                sapi.BroadcastMessageToAllGroups($"{@event.raidChannel} is raiding with {@event.numberOfViewers} viewiers !", EnumChatType.Notification);
            }
        }

        private void OnTwitchBitsMessage(IServerPlayer player, BitsData @event)
        {
            if (ConfigData.PlayerIsAllowed(player))
            {
                sapi.BroadcastMessageToAllGroups($"{@event.from} gave {@event.amount} with message {@event.message}", EnumChatType.Notification);
            }
        }

        private void OnTwitchFollowMessage(IServerPlayer player, FollowData @event)
        {
            if (ConfigData.PlayerIsAllowed(player))
            {
                sapi.BroadcastMessageToAllGroups($"{@event.who} is now Following {@event.channel}!", EnumChatType.Notification);
            }
        }

        private void OnTwitchNewSubMessage(IServerPlayer player, NewSubData @event)
        {
            if (ConfigData.PlayerIsAllowed(player))
            {
                if (@event.isGift)
                {
                    sapi.BroadcastMessageToAllGroups($"{@event.from} Gifted Sub to {@event.to}!", EnumChatType.Notification);
                }
                else
                {
                    sapi.BroadcastMessageToAllGroups($"{@event.to} Subscribed with message {@event.message}", EnumChatType.Notification);
                }
            }
        }

        private void OnTwitchPointRedemptionMessage(IServerPlayer player, PointRedemptionData @event)
        {
            if (ConfigData.PlayerIsAllowed(player))
            {
                sapi.BroadcastMessageToAllGroups($"{@event.who} redeemed {@event.redemptionName}", EnumChatType.Notification);
            }
        }

        #region Client
        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            capi = api;

            api.Event.RegisterEventBusListener(OnEvent);
        }

        private void OnEvent(string eventName, ref EnumHandling handling, IAttribute data)
        {
            switch (eventName)
            {
                case Constants.EVENT_BITS_RECIEVED:
                    capi.Network.GetChannel(Constants.NETWORK_EVENT_CHANNEL).SendPacket(data.GetValue() as BitsData);
                    break;
                case Constants.EVENT_FOLLOW:
                    capi.Network.GetChannel(Constants.NETWORK_EVENT_CHANNEL).SendPacket(data.GetValue() as FollowData);
                    break;
                case Constants.EVENT_REDEMPTION:
                    capi.Network.GetChannel(Constants.NETWORK_EVENT_CHANNEL).SendPacket(data.GetValue() as PointRedemptionData);
                    break;
                case Constants.EVENT_RAID:
                    capi.Network.GetChannel(Constants.NETWORK_EVENT_CHANNEL).SendPacket(data.GetValue() as RaidData);
                    break;
                case Constants.EVENT_NEW_SUB:
                    capi.Network.GetChannel(Constants.NETWORK_EVENT_CHANNEL).SendPacket(data.GetValue() as NewSubData);
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
