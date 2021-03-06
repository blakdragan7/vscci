namespace VSCCI.Data
{
    using System.IO;
    using System.Collections.Generic;
    using Vintagestory.API.Server;

    using System;
    using Newtonsoft.Json.Linq;
    using VSCCI.CCIIntegrations;

    public class ServerConfigData
    {
        public List<string> Whitelist { get; } = new List<string>();
    }

    public class ClientConfigData
    {
        public bool PlayerIsAllowedServerEvents;
        public CCIType PlatformType;

        public ClientConfigData()
        {
            PlayerIsAllowedServerEvents = false;
            PlatformType = CCIType.Twitch;
        }
    }

    public class ConfigData
    {
        // only available server side
        private static ServerConfigData serverData = null;

        // accessable server side but pointless
        public static ClientConfigData clientData = new ClientConfigData();

        public static bool LoadConfig(ICoreServerAPI api)
        {
            try
            {
                serverData = api.LoadModConfig<ServerConfigData>(Constants.CONFIG_FILE);

                if(serverData == null)
                {
                    LoadConfigObsolete(api);

                    api.StoreModConfig(serverData, Constants.CONFIG_FILE);
                }
            }
            catch(Exception exc)
            {
                api.Logger.Error("Error trying to load server config file {0}", exc.Message);
            }
            return false;
        }

        public static bool LoadConfigObsolete(ICoreServerAPI api)
        {
            serverData = new ServerConfigData();

            try
            {
                var configData = File.ReadAllText(Constants.CONFIG_FILE_PATH_OBSELETE);
                var token = JObject.Parse(configData);
                var vscci = token.SelectToken("vscci");
                var whitelist = vscci.SelectToken("whitelist");
                var arr = whitelist.ToObject<string[]>();

                foreach(var player in arr)
                {
                    serverData.Whitelist.Add(player);
                }

                return true;
            }
            catch (Exception)
            {
                // just ignore
                return false;
            }
        }

        public static bool PlayerIsAllowed(IServerPlayer player)
        {
            return serverData.Whitelist.Contains(player.PlayerName);
        }
    }
}
