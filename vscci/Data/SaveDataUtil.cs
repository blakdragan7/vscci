namespace VSCCI.Data
{
    using System.IO;
    using System.Collections.Generic;

    using Newtonsoft.Json.Linq;
    using Vintagestory.API.Common;
    using Vintagestory.API.Client;

    public class ClientSaveData
    {
        public string TwitchAuth;
        public string StreamlabsAuth;
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
                    { "streamlabs", data.StreamlabsAuth}
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
                        data.StreamlabsAuth = obj.SelectToken("streamlabs")?.ToString();
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
    }
}
