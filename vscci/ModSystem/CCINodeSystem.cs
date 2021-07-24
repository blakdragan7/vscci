namespace VSCCI.ModSystem
{
    using Vintagestory.API.Common;
    using Vintagestory.API.Server;
    using Vintagestory.API.Client;
    using Vintagestory.API.Datastructures;

    using VSCCI.Data;
    class CCINodeSystem : ModSystem
    {
        private ICoreServerAPI sapi;
        private ICoreClientAPI capi;

        private static CCINodeSystem instance = null;

        public static CCINodeSystem NodeSystem => instance;

        public override void Start(ICoreAPI api)
        {
            instance = this;

            base.Start(api);
            api.Network.RegisterChannel(Constants.NETWORK_NODE_CHANNEL);
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
    }
}
