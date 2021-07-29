namespace VSCCI.CCINetworkTypes
{
    using ProtoBuf;
    using VSCCI.CCIIntegrations;
    using System.Collections.Generic;

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CCILoginRequest
    {
        public List<CCIType> type;
        public string data;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class TwitchLoginStep
    {
        public string url;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class TwitchLoginStepResponse
    {
        public string authToken;
        public bool success;
        public string error;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CCIConnectRequest
    {
        public string twitchid;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CCIRequestResponse
    {
        public string requestType;
        public string response;
        public bool success;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CCILoginUpdate
    {
        public string user;
        public string id;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CCIServerEventStatusUpdate
    {
        public string status;
        public bool isAllowed;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CCIConnectionUpdate
    {
        public CCIType type;
        public string status;
    }
}
