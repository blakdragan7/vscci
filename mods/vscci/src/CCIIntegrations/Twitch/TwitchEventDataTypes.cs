using ProtoBuf;

namespace vscci.src.CCIIntegrations.Twitch
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class TwitchBitsData
    {
        public int amount;
        public string from;
        public string message;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class TwitchNewSubData
    {
        public string from; // this is null if isGift is false
        public bool isGift;
        public string to;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class TwitchRaidData
    {
        public string raidChannel;
        public int numberOfViewers;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class TwitchPointRedemptionData
    {
        public string who;
        public string message;
        public string redemptionName;
        public string redemptionID;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class TwitchFollowData
    {
        public string who;
    }
}
