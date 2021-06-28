using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vintagestory.API.Server;
using Vintagestory.API.Util;
using vscci.src.CCIIntegrations.Twitch;

namespace vscci.src.Data
{
    class SaveDataUtil
    {
        public static void SaveAuthData(ICoreServerAPI api, Dictionary<IServerPlayer, TwitchIntegration> dti)
        {
            Dictionary<string, string> sdti = new Dictionary<string, string>();

            foreach(var pair in dti)
            {
                sdti.Add(pair.Key.PlayerUID, pair.Value.GetAuthDataForSaving());
            }

            api.WorldManager.SaveGame.StoreData(Constants.TWITH_AUTH_SAVE_TAG, SerializerUtil.Serialize<Dictionary<string,string>>(sdti));
        }

        public static void LoadAuthData(ICoreServerAPI api,VSCCIModSystem vscci)
        {
            byte[] data = api.WorldManager.SaveGame.GetData(Constants.TWITH_AUTH_SAVE_TAG);

            if(data != null)
            {
                var sdti = SerializerUtil.Deserialize<Dictionary<string, string>>(data);

                foreach (var pair in sdti)
                {
                    if (pair.Value != null)
                    {
                        IServerPlayer player = Array.Find(api.Server.Players, delegate (IServerPlayer p) { return p.PlayerUID == pair.Key; });

                        TwitchIntegration ti = vscci.TIForPlayer(player);
                        ti.SetAuthDataFromSaveData(pair.Value);
                    }
                }
            }
        }
    }
}
