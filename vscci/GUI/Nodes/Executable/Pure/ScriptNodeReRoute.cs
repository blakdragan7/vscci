namespace VSCCI.GUI.Nodes
{ 
    using System;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Nodes.Attributes;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Pins;
    using System.IO;

    [NodeData("Util", "Re-Route")]
    [InputPin(typeof(DynamicType), 0)]
    [OutputPin(typeof(DynamicType), 0)]
    public class ScriptNodeReRoute : ExecutableScriptNode
    {
        Type type;

        public ScriptNodeReRoute(ICoreClientAPI api, MatrixElementBounds bounds) : base("", api, bounds, true)
        {
            type = null;
        }

        public ScriptNodeReRoute(ICoreClientAPI api, Type pinType, MatrixElementBounds bounds) : base("", api, bounds, true)
        {
            type = pinType;

            if (pinType == typeof(Exec))
            {
                inputs.Add(new ExecInputNode(this, ""));
                outputs.Add(new ExecOutputNode(this, ""));
            }
            else
            {
                inputs.Add(new ScriptNodeInput(this, "", pinType));
                outputs.Add(new ScriptNodeOutput(this, "", pinType));
            }
        }

        protected override void OnExecute()
        {
            if(type == typeof(Exec))
            {
                ExecuteOutput(outputs[0]);
            }
            else
            {
                outputs[0].Value = inputs[0].GetInput();
            }
        }

        public override string GetNodeDescription()
        {
            return "";
        }

        public override void WrtiePinsToBytes(BinaryWriter writer)
        {
            writer.Write(type.AssemblyQualifiedName);
            inputs[0].ToBytes(writer);
            outputs[0].ToBytes(writer);
        }

        public override void ReadPinsFromBytes(BinaryReader reader)
        {
            var typeString = reader.ReadString();
            type = Type.GetType(typeString);

            if (type == typeof(Exec))
            {
                inputs.Add(new ExecInputNode(this, ""));
                outputs.Add(new ExecOutputNode(this, ""));
            }
            else
            {
                inputs.Add(new ScriptNodeInput(this, "", type));
                outputs.Add(new ScriptNodeOutput(this, "", type));
            }

            base.ReadPinsFromBytes(reader);
        }
    }
}
