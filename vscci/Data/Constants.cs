namespace vscci.Data
{
    public class Constants
    {
        public const int PROTO_TYPE_ATTRIBUTE_ID            = 100;
        public const string CLIENT_SAVE_FILE                = "vscci_data.json";
        public const string CONFIG_FILE                     = "vscci.json";
        public const string CONFIG_FILE_DIR_OBSELETE        = "data/ModConfig";
        public const string CONFIG_FILE_PATH_OBSELETE       = "data/ModConfig/vscci_config.json";

        // GUI Rendering Constants
        public const double NODE_SCIPRT_TEXT_PADDING        = 10.0;
        public const string CCI_EVENT_SERVER_UPDATE         = "ccisu";
        public const string CCI_EVENT_LOGIN_UPDATE          = "ccilu";
        public const string CCI_EVENT_CONNECT_UPDATE        = "ccicu";
        public const string CCI_EVENT_LOGIN_REQUEST         = "ccilr";
        public const string CCI_EVENT_DISCONNECT_REQUEST    = "ccidr";
        public const string CCI_EVENT_SERVER_UPDATE_REQUEST = "ccisr";


        // Server => Client Communication Constants
        public const string NETWORK_CHANNEL                = "cci_message_channel";
        public const string NETWORK_GUI_CHANNEL            = "cci_guiupdate_channel";
        public const string NETWORK_EVENT_CHANNEL          = "cci_event_channel";

        // CCI Event Constants
        public const string EVENT_BITS_RECIEVED            = "ccieb";
        public const string EVENT_NEW_SUB                  = "ccies";
        public const string EVENT_RAID                     = "ccier";
        public const string EVENT_REDEMPTION               = "cciee";
        public const string EVENT_FOLLOW                   = "ccief";
        public const string EVENT_DONATION                 = "ccied";
        public const string EVENT_HOST                     = "ccieh";
        public const string EVENT_SCHAT                    = "cciesc";

        // Twitch Integration Constants
        public const string TWITH_AUTH_SAVE_TAG            = "vscci_tia_data";
        public const string TWITCH_ID_URL                  = "https://id.twitch.tv/oauth2/authorize?";
        public const string TWITCH_VALIDATE_URL            = "https://id.twitch.tv/oauth2/validate";
        public const string TWITCH_CHANNELS_URL            = "https://api.twitch.tv/kraken/channels/";
        public const string TWITCH_CLIENT_ID               = "izjwtydb6a3i11ftc5uewgc2gzjbow";
        public const string TWITCH_REDIRECT_URI            = "http://localhost:4444/implicit";
        public const string LISTEN_PREFIX                  = "http://localhost:4444/";
        public const int AUTH_VALIDATION_INTERVAL          = 10000;

        // StreamLabs Constants
        public const string SL_SOCKET_API_URL              = "https://sockets.streamlabs.com/token=";
    }
}
