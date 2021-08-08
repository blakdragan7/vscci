namespace VSCCI.GUI.Pins
{
    using Cairo;
    using VSCCI.GUI.Nodes;
    class ExecInputNode : ScriptNodeInput
    { 
        public ExecutableScriptNode ExecOwner => this.owner as ExecutableScriptNode;
        public ExecInputNode(ExecutableScriptNode owner, string name = "exec") : base(owner, name, typeof(Exec))
        {

        }
        public void SetPinRunning(bool running)
        {
            color = running ? new Color(1.0, 0.0, 0.0, 1.0) : ColorForValueType(pinValueType);
        }
    }
}
