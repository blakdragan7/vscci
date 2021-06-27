using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProtoBuf;
namespace vscci.src.CCINetworkTypes
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CCILoginRequest
    {
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CCILoginStep
    {
        public string url;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CCIConnectRequest
    {
        public string twitchid;
        public bool istwitchpartner;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CCIRequestResponse
    {
        public string requestType;
        public string response;
        public bool success;
    }
}
