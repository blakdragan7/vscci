namespace VSCCI.ModSystem
{
    using Vintagestory.API.Client;
    using Vintagestory.API.Server;
    using Vintagestory.API.Common;
    using Vintagestory.API.Datastructures;

    using VSCCI.Data;
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
            /*if (ConfigData.PlayerIsAllowed(player))
            {
            }*/
        }

        private void OnTwitchBitsMessage(IServerPlayer player, BitsData @event)
        {

        }

        private void OnTwitchFollowMessage(IServerPlayer player, FollowData @event)
        {

        }

        private void OnTwitchNewSubMessage(IServerPlayer player, NewSubData @event)
        {

        }

        private void OnTwitchPointRedemptionMessage(IServerPlayer player, PointRedemptionData @event)
        {

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
