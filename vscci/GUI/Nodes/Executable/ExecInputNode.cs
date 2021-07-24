using System;
using System.Collections.Generic;
using System.Text;

namespace VSCCI.GUI.Nodes
{
    class ExecInputNode : ScriptNodeInput
    { 
        public ExecutableScriptNode ExecOwner => this.owner as ExecutableScriptNode;
        public ExecInputNode(ExecutableScriptNode owner, string name = "exec") : base(owner, name, typeof(Exec))
        {

        }
    }
}
