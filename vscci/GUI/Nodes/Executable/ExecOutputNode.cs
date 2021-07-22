namespace vscci.GUI.Nodes
{
    class ExecOutputNode : ScriptNodeOutput
    {
        public ExecutableScriptNode ExecOwner => this.owner as ExecutableScriptNode;
        public ExecOutputNode(ExecutableScriptNode owner, string name = "exec") : base(owner, name, 1, typeof(Exec))
        {

        }
    }
}
