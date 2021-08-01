namespace VSCCI.GUI.Nodes
{
    using Cairo;
    class ExecOutputNode : ScriptNodeOutput
    {
        public ExecutableScriptNode ExecOwner => this.owner as ExecutableScriptNode;
        public ExecOutputNode(ExecutableScriptNode owner, string name = "exec") : base(owner, name, typeof(Exec), 1)
        {

        }

        public void SetPinRunning(bool running)
        {
            color = running ? new Color(1.0, 0.0, 0.0, 1.0) : ColorForValueType(pinValueType);
        }
    }
}
