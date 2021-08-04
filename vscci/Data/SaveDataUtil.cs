namespace VSCCI.Data
{
    using System.IO;
    using System.Collections.Generic;

    using Newtonsoft.Json.Linq;
    using Vintagestory.API.Common;
    using Vintagestory.API.Client;
    using VSCCI.CCIIntegrations;

    public class ClientSaveData
    {
        public string TwitchAuth;
        public CCIType PlatformType;
        public string PlatformAuth;
    }

    public class SaveDataUtil
    {
        public static void SaveClientData(ICoreClientAPI api, ClientSaveData data)
        {
            try
            {
                var d = new Dictionary<string, string>
                {
                    { "auth", data.TwitchAuth },
                    { "platform", StringFromCCIType(data.PlatformType)},
                    { "platform_auth", data.PlatformAuth}
                };

                var jsonData = JsonUtil.ToString(d);
                File.WriteAllText(Constants.CLIENT_SAVE_FILE, jsonData);
            }
            catch (IOException exception)
            {
                api.Logger.Error("Could Not Save Client Save Data With Exception {0}", exception.Message);
            }
        }

        public static ClientSaveData LoadClientData(ICoreClientAPI api)
        {
            var data = new ClientSaveData();
            try
            {
                var path = Path.GetFullPath(Constants.CLIENT_SAVE_FILE);
                var jsonData = File.ReadAllText(Constants.CLIENT_SAVE_FILE);
                if (jsonData != null)
                {
                    var obj = JObject.Parse(jsonData);

                    if (obj != null)
                    {
                        data.TwitchAuth = obj.SelectToken("auth")?.ToString();
                        data.PlatformType = CCITypeFromString(obj.SelectToken("platform")?.ToString());
                        data.PlatformAuth = obj.SelectToken("platform_auth")?.ToString();
                        var oldData = obj.SelectToken("streamlabs")?.ToString();
                        if(oldData != null && oldData.Length > 0)
                        {
                            data.PlatformType = CCIType.Streamlabs;
                            data.PlatformAuth = oldData;
                        }
                    }
                }
            }
            catch(IOException exception)
            {
                // catch the no file exception because it doesnt matter
                api.Logger.Warning("Could Not Load Client Save Data With Exception {0}", exception.Message);
            }

            return data;
        }

        public static string StringFromCCIType(CCIType type)
        {
            switch(type)
            {
                case CCIType.Twitch:
                    return "Twitch";
                case CCIType.Streamlabs:
                    return "Streamlabs";
                case CCIType.Streamelements:
                    return "Streamelements";
                default:
                    return "Invalid";
            }
        }

        public static CCIType CCITypeFromString(string type)
        {
            switch (type)
            {
                case "Twitch":
                    return CCIType.Twitch;
                case "Streamlabs":
                    return CCIType.Streamlabs;
                case "Streamelements":
                    return CCIType.Streamelements;
                default:
                    return CCIType.Twitch;
            }

        }
    }
}
