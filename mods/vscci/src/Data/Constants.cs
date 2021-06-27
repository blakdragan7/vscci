using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vscci.src.Data
{
    class Constants
    {
        public const int PROTO_TYPE_ATTRIBUTE_ID           = 100;

        // Server => Client Communication Constants
        public const string NETWORK_CHANNEL                = "cci_message_channel";

        // Twitch Event Constants
        public const string TWITCH_EVENT_BITS_RECIEVED     = "ccieb";
        public const string TWITCH_EVENT_NEW_SUB           = "ccies";
        public const string TWITCH_EVENT_RAID              = "ccier";
        public const string TWITCH_EVENT_REDEMPTION        = "cciee";
        public const string TWITCH_EVENT_FOLLOW            = "ccief";

        // Twitch Integration Constants
        public const string TWITH_AUTH_SAVE_TAG            = "vscci_tia_data";
        public const string TWITCH_ID_URL                  = "https://id.twitch.tv/oauth2/authorize?";
        public const string TWITCH_VALIDATE_URL            = "https://id.twitch.tv/oauth2/validate";
        public const string TWITCH_CLIENT_ID               = "izjwtydb6a3i11ftc5uewgc2gzjbow";
        public const string TWITCH_REDIRECT_URI            = "http://localhost:4444/implicit";
        public const string LISTEN_PREFIX                  = "http://localhost:4444/";
        public const int AUTH_VALIDATION_INTERVAL          = 10000;
    }
}
