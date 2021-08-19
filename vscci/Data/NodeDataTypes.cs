
namespace VSCCI.Data
{
    using ProtoBuf;
    using System;
    using Vintagestory.API.MathTools;

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class ServerNodeExecutionData
    {
        public string AssemblyQualifiedName;
        public string Data;
        public Guid Guid;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PlayerPositionData
    {
        public Vec3d Position;
        public Guid Guid;
    }
}
