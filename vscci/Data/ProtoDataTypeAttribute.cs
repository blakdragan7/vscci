namespace VSCCI.Data
{
    using System.IO;

    using Vintagestory.API.Util;
    using Vintagestory.API.Common;
    using Vintagestory.API.Datastructures;

    public class ProtoDataTypeAttribute<T> : IAttribute
    {
        public T value;
        public ProtoDataTypeAttribute()
        {

        }

        public ProtoDataTypeAttribute(T value)
        {
            this.value = value;
        }

        public bool Equals(IWorldAccessor worldForResolve, IAttribute attr)
        {
            return attr.GetValue() == value as object;
        }

        public IAttribute Clone()
        {
            return new ProtoDataTypeAttribute<T>(value);
        }

        public int GetAttributeId()
        {
            return Constants.PROTO_TYPE_ATTRIBUTE_ID;
        }

        public object GetValue()
        {
            return value;
        }

        public void FromBytes(BinaryReader stream)
        {
            int l = stream.ReadInt32();
            var b = stream.ReadBytes(l);

            if(b != null && b.Length > 0)
            {
                value = SerializerUtil.Deserialize<T>(b);
            }
        }

        public void ToBytes(BinaryWriter stream)
        {
            var b = SerializerUtil.Serialize<T>(value);
            int l =b.Length;

            stream.Write(l);
            stream.Write(b);
        }

        public string ToJsonToken()
        {
            return JsonUtil.ToString<T>(value);
        }
    }
}
