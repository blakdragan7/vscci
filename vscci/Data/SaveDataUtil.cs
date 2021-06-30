namespace vscci.Data
{
    using System;
    using System.Collections.Generic;

    using Vintagestory.API.Server;
    using Vintagestory.API.Util;

    using vscci.CCIIntegrations.Twitch;
    using vscci.ModSystem;

    class SaveDataUtil
    {
        public static void SaveAuthData(ICoreServerAPI api, Dictionary<IServerPlayer, TwitchIntegration> dti, Dictionary<string, string> notFoundSavedData)
        {
            Dictionary<string, string> sdti = new Dictionary<string, string>();

            foreach(var pair in dti)
            {
                sdti.Add(pair.Key.PlayerUID, pair.Value.GetAuthDataForSaving());
            }

            if (notFoundSavedData != null)
            {
                foreach (var pair in notFoundSavedData)
                {
                    sdti.Add(pair.Key, pair.Value);
                }
            }

            api.WorldManager.SaveGame.StoreData(Constants.TWITH_AUTH_SAVE_TAG, SerializerUtil.Serialize<Dictionary<string,string>>(sdti));
        }

        public static void LoadAuthData(ICoreServerAPI api,VSCCIModSystem vscci, ref Dictionary<string, string> notFoundSavedData)
        {
            byte[] data = api.WorldManager.SaveGame.GetData(Constants.TWITH_AUTH_SAVE_TAG);
            List<string> toRemove = new List<string>();

            if(data != null)
            {
                var sdti = SerializerUtil.Deserialize<Dictionary<string, string>>(data);

                foreach (var pair in sdti)
                {
                    if (pair.Value != null)
                    {
                        IServerPlayer player = Array.Find(api.Server.Players, delegate (IServerPlayer p) { return p.PlayerUID == pair.Key; });

                        if (player != null)
                        {
                            toRemove.Add(pair.Key);
                            TwitchIntegration ti = vscci.TIForPlayer(player);
                            ti.SetAuthDataFromSaveData(pair.Value);
                        }
                    }
                }

                foreach(var uuid in toRemove)
                {
                    sdti.Remove(uuid);
                }

                if(sdti.Count > 0)
                    notFoundSavedData = sdti;
            }
        }
    }
}
