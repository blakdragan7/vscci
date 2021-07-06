namespace vscci.Data
{
    using System.IO;
    using System.Collections.Generic;
    using Vintagestory.API.Server;

    using Vintagestory.API.Common;
    using System;
    using Newtonsoft.Json.Linq;

    class ConfigData
    {
        public static List<string> VSCCIAllowedPlayers { get; } = new List<string>();

        public static void LoadConfig(ICoreServerAPI api)
        {
            try
            {
                var configData = File.ReadAllText(Constants.CONFIG_FILE_PATH);
                var token = JObject.Parse(configData);
                var vscci = token.SelectToken("vscci");
                var whitelist = vscci.SelectToken("whitelist");
                var arr = whitelist.ToObject<string[]>();

                foreach(var player in arr)
                {
                    VSCCIAllowedPlayers.Add(player);
                }

            }
            catch(FileNotFoundException)
            {
                // generate empty config file

                var di = new Dictionary<string, object>();
                var root = new Dictionary<string, string[]>
                {
                    { "whitelist", new string[]{ } }
                };
                di.Add("vscci", root);

                try
                {

                    File.WriteAllText(Constants.CONFIG_FILE_PATH, JsonUtil.ToString(di));
                }
                catch (Exception exc)
                {
                    api.Logger.Error("Could not make default config file {0}", exc.Message);
                }

            }
            catch(DirectoryNotFoundException)
            {
                try
                {
                    Directory.CreateDirectory(Constants.CONFIG_FILE_DIR);
                }
                catch (Exception exc)
                {
                    api.Logger.Error("Could not make default config directory {0}", exc.Message);
                    return;
                }

                var di = new Dictionary<string, object>();
                var root = new Dictionary<string, string[]>
                {
                    { "whitelist", new string[]{ } }
                };
                di.Add("vscci", root);

                try
                {
                    File.WriteAllText(Constants.CONFIG_FILE_PATH, JsonUtil.ToString(di));
                }
                catch (Exception exc)
                {
                    api.Logger.Error("Could not make default config file {0}", exc.Message);
                }
            }
            catch (Exception exc)
            {
                // just ignore
                api.Logger.Error("Could not read config file {0}", exc.Message);
            }
        }

        public static bool PlayerIsAllowed(IServerPlayer player)
        {
            return VSCCIAllowedPlayers.Contains(player.PlayerName);
        }
    }
}
