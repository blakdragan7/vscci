
namespace VSCCI.Data
{
    using ProtoBuf;

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    class ServerNodeExecutionData
    {
        public string AssemblyQualifiedName;
        public string Data;
    }
}
