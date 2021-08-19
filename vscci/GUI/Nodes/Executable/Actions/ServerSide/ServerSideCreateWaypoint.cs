namespace VSCCI.GUI.Nodes.Executable.Actions
{
    using VSCCI.GUI.Pins;
    using VSCCI.GUI.Nodes.Attributes;
    using VSCCI.GUI.Elements;

    using Vintagestory.API.Client;
    using Vintagestory.API.Server;
    using VSCCI.Data;
    using Vintagestory.API.Config;
    using Vintagestory.API.MathTools;

    public class CreateWaypointAction : ServerSideAction
    {
        public override void RunServerSide(IServerPlayer player, ICoreServerAPI api, string data)
        {
        }
    }

    [NodeData("Actions", "Create Waypoint")]
    [InputPin(typeof(Exec), 0)]
    [InputPin(typeof(string), 1)]
    [InputPin(typeof(bool), 2)]
    [InputPin(typeof(Vec3d), 3)]
    [OutputPin(typeof(Exec), 0)]
    public class ServerSideCreateWaypoint : ServerSideExecutableNode<CreateWaypointAction>
    {
        ScriptNodeInput nameInput;
        ScriptNodeInput pinnedInput;
        ScriptNodeInput positionInput;

        public ServerSideCreateWaypoint(ICoreClientAPI api, MatrixElementBounds bounds) : base("Create Waypoint", api, bounds)
        {
            nameInput = new ScriptNodeInput(this, "Name", typeof(string));
            pinnedInput = new ScriptNodeBoolInput(this, "Pinned");
            positionInput = new ScriptNodeInput(this, "Position", typeof(Vec3d));

            inputs.Add(nameInput);
            inputs.Add(pinnedInput);
            inputs.Add(positionInput);
        }

        protected override void OnExecute()
        {
            string name = nameInput.GetInput();
            bool pinned = pinnedInput.GetInput();

            var colorText = "white";
            var icon = "default";

            Vec3d WorldPosd = positionInput.GetInput();
            Vec3i WorldPos = new Vec3i(WorldPosd.XInt, WorldPosd.YInt, WorldPosd.ZInt);

            if (ConfigData.clientData.PlayerIsAllowedServerEvents)
            {
                api.SendChatMessage(string.Format("/waypoint addati {0} ={1} ={2} ={3} {4} {5} {6}", icon, WorldPos.X.ToString(GlobalConstants.DefaultCultureInfo), WorldPos.Y.ToString(GlobalConstants.DefaultCultureInfo), WorldPos.Z.ToString(GlobalConstants.DefaultCultureInfo), pinned, colorText, name));
            }
        }

        public override string GetNodeDescription()
        {
            return "Creates a waypoint on the given players map";
        }
    }
}
