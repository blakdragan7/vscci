namespace VSCCI.GUI.Nodes
{
    using System;
    using System.IO;
    using System.Threading;
    using Vintagestory.API.Client;
    using Vintagestory.API.MathTools;
    using Vintagestory.API.Server;
    using VSCCI.Data;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes.Attributes;
    using VSCCI.GUI.Pins;
    using VSCCI.ModSystem;

    public class ServerSideGetNamedPlayerAction : ServerSideAction
    {
        public override void RunServerSide(IServerPlayer player, ICoreServerAPI api, ServerNodeExecutionData data)
        {
            foreach(var onlinePlayer in api.World.AllOnlinePlayers)
            {
                if(onlinePlayer.PlayerName.ToLower() == data.Data.ToLower())
                {
                    api.Network.GetChannel(Constants.NETWORK_NODE_CHANNEL).SendPacket(new PlayerPositionData()
                    {
                        Guid = data.Guid,
                        Position = onlinePlayer.Entity.Pos.XYZ
                    }, player);

                    return;
                }
            }

            api.Network.GetChannel(Constants.NETWORK_NODE_CHANNEL).SendPacket(new PlayerPositionData()
            {
                Guid = data.Guid,
                Position = new Vec3d(0, 0, 0)
            }, player);
        }
    }

    [NodeData("Util", "Named Player Position")]
    [InputPin(typeof(Exec), 0)]
    [InputPin(typeof(string), 1)]
    [OutputPin(typeof(Exec), 0)]
    [OutputPin(typeof(Vec3d), 1)]
    public class GetNamedPlayerPosition : ServerSideExecutableNode
    {
        private readonly AutoResetEvent _signal = new AutoResetEvent(false);

        public GetNamedPlayerPosition(ICoreClientAPI api, MatrixElementBounds bounds) : base("Get Named Player Position", typeof(ServerSideGetNamedPlayerAction), api, bounds)
        {
            inputs.Add(new ScriptNodeTextInput(this, typeof(string)));
            outputs.Add(new ScriptNodeOutput(this, "Position", typeof(Vec3d)));

            outputs[1].Value = new Vec3d();

            CCINodeSystem.NodeSystem.RegisterNodeForServerMessages(this);
        }

        protected override void OnExecute()
        {
            data = inputs[1].GetInput();

            base.OnExecute();

            // wait to receive the player position
            _signal.WaitOne(Constants.NETWORK_DEFAULT_TIMEUOT_MS);
        }

        public override void ReceivedServerMessage(object data)
        {
            api.ShowChatMessage("ReceivedServerMessage");

            var positionData = data as PlayerPositionData;
            if (positionData != null)
            {
                outputs[1].Value = positionData.Position;
            }
            else
            {
                api.Logger.Warning("GetNamedPlayerPosition Received invalid data from server");
            }

            _signal.Set();
        }

        public override void SetGuid(Guid newGuid)
        {
            CCINodeSystem.NodeSystem.UnregisterNodeForServerMessages(this);
            base.SetGuid(newGuid);
            CCINodeSystem.NodeSystem.RegisterNodeForServerMessages(this);
        }

        public override string GetNodeDescription()
        {
            return "This returns the current position of the player with the username given to \"Name\" or position {0, 0, 0} if that player is not found";
        }

        public override void Dispose()
        {
            base.Dispose();

            CCINodeSystem.NodeSystem.UnregisterNodeForServerMessages(this);
        }
    }
}
