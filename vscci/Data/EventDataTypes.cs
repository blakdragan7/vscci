using ProtoBuf;
using Vintagestory.API.Server;

namespace vscci.Data
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class BitsData
    {
        public int amount;
        public string from;
        public string message;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DonationData
    {
        public string who;
        public string amount;
        public string message;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class NewSubData
    {
        public string from; // this is null if isGift is false
        public bool isGift;
        public string to;
        public string message;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class RaidData
    {
        public string raidChannel;
        public int numberOfViewers;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PointRedemptionData
    {
        public string who;
        public string message;
        public string redemptionName;
        public string redemptionID;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class FollowData
    {
        public string who;
        public string channel;
        public string platform;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class HostData
    {
        public string who;
        public int viewers;
    }
}
