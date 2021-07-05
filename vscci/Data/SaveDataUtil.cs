namespace vscci.Data
{
    using System.IO;
    using System.Collections.Generic;

    using Newtonsoft.Json.Linq;
    using Vintagestory.API.Common;
    using Vintagestory.API.Client;

    public class SaveDataUtil
    {
        public static void SaveClientData(ICoreClientAPI api, string token)
        {
            try
            {
                var d = new Dictionary<string, string>
                {
                    { "auth", token }
                };

                var jsonData = JsonUtil.ToString(d);
                File.WriteAllText(Constants.CLIENT_SAVE_FILE, jsonData);
            }
            catch (IOException exception)
            {
                api.Logger.Error("Could Not Save Client Save Data With Exception {0}", exception.Message);
            }
        }

        public static void LoadClientData(ICoreClientAPI api, ref string token)
        {
            try
            {
                var jsonData = File.ReadAllText(Constants.CLIENT_SAVE_FILE);
                if (jsonData != null)
                {
                    var obj = JObject.Parse(jsonData);

                    if (obj != null)
                    {
                        var authObject = obj.SelectToken("auth");
                        if (authObject != null)
                        {
                            token = authObject.ToString();
                        }
                    }
                }
            }
            catch(IOException exception)
            {
                // catch the no file exception because it doesnt matter
                api.Logger.Warning("Could Not Load Client Save Data With Exception {0}", exception.Message);
            }
        }
    }
}
