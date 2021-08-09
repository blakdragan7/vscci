namespace VSCCI.GUI.Nodes.Executable.Pure
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.MathTools;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes.Attributes;
    using VSCCI.GUI.Pins;

    [NodeData("Util", "Current Player Position")]
    [InputPin(typeof(Number), 0)]
    [InputPin(typeof(Number), 1)]
    [OutputPin(typeof(Number), 0)]
    class CurrentPlayerPosition : ExecutableScriptNode
    {
        public static int OUTPUT_INDEX = 0;
        public CurrentPlayerPosition(ICoreClientAPI api, MatrixElementBounds bounds) : base("Current Player Position", api, bounds, true)
        {
            outputs.Add(new ScriptNodeOutput(this, "Position", typeof(Vec3d)));

            shouldAutoExecuteNext = false;
        }

        protected override void OnExecute()
        {
            outputs[OUTPUT_INDEX].Value = api.World.Player.Entity.Pos.XYZ;
        }

        public override string GetNodeDescription()
        {
            return "This get the current players position";
        }
    }
}
