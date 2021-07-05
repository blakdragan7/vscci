namespace vscci.ModSystem
{
    using Vintagestory.API.Client;
    using Vintagestory.API.Server;
    using Vintagestory.API.Common;
    using Vintagestory.API.Datastructures;

    using vscci.Data;
    using vscci.CCIIntegrations.Twitch;
    public class TwitchEventSystem : ModSystem
    {
        private ICoreClientAPI capi;
        private ICoreServerAPI sapi;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.Network.RegisterChannel(Constants.NETWORK_EVENT_CHANNEL)
                .RegisterMessageType(typeof(TwitchRaidData))
                .RegisterMessageType(typeof(TwitchBitsData))
                .RegisterMessageType(typeof(TwitchFollowData))
                .RegisterMessageType(typeof(TwitchNewSubData))
                .RegisterMessageType(typeof(TwitchPointRedemptionData));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            sapi = api;

            api.Network.GetChannel(Constants.NETWORK_EVENT_CHANNEL)
                .SetMessageHandler<TwitchRaidData>(OnTwitchRaidMessage)
                .SetMessageHandler<TwitchBitsData>(OnTwitchBitsMessage)
                .SetMessageHandler<TwitchFollowData>(OnTwitchFollowMessage)
                .SetMessageHandler<TwitchNewSubData>(OnTwitchNewSubMessage)
                .SetMessageHandler<TwitchPointRedemptionData>(OnTwitchPointRedemptionMessage);
        }

        private void OnTwitchRaidMessage(IServerPlayer player, TwitchRaidData @event)
        {
            sapi.BroadcastMessageToAllGroups($"{@event.raidChannel} is raiding with {@event.numberOfViewers} viewiers !", EnumChatType.Notification);
        }

        private void OnTwitchBitsMessage(IServerPlayer player, TwitchBitsData @event)
        {
            sapi.BroadcastMessageToAllGroups($"{@event.from} gave {@event.amount} with message {@event.message}", EnumChatType.Notification);
        }

        private void OnTwitchFollowMessage(IServerPlayer player, TwitchFollowData @event)
        {
            sapi.BroadcastMessageToAllGroups($"{@event.who} is now Following {@event.channel}!", EnumChatType.Notification);
        }

        private void OnTwitchNewSubMessage(IServerPlayer player, TwitchNewSubData @event)
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

        private void OnTwitchPointRedemptionMessage(IServerPlayer player, TwitchPointRedemptionData @event)
        {
            sapi.BroadcastMessageToAllGroups($"{@event.who} redeemed {@event.redemptionName}", EnumChatType.Notification);
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
                case Constants.TWITCH_EVENT_BITS_RECIEVED:
                    capi.Network.GetChannel(Constants.NETWORK_EVENT_CHANNEL).SendPacket(data.GetValue() as TwitchBitsData);
                    break;
                case Constants.TWITCH_EVENT_FOLLOW:
                    capi.Network.GetChannel(Constants.NETWORK_EVENT_CHANNEL).SendPacket(data.GetValue() as TwitchFollowData);
                    break;
                case Constants.TWITCH_EVENT_REDEMPTION:
                    capi.Network.GetChannel(Constants.NETWORK_EVENT_CHANNEL).SendPacket(data.GetValue() as TwitchPointRedemptionData);
                    break;
                case Constants.TWITCH_EVENT_RAID:
                    capi.Network.GetChannel(Constants.NETWORK_EVENT_CHANNEL).SendPacket(data.GetValue() as TwitchRaidData);
                    break;
                case Constants.TWITCH_EVENT_NEW_SUB:
                    capi.Network.GetChannel(Constants.NETWORK_EVENT_CHANNEL).SendPacket(data.GetValue() as TwitchNewSubData);
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
