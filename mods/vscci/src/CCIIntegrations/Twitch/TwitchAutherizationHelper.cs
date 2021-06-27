using System.Collections.Generic;
using System.Net;
using TwitchLib.Api.Core.Enums;
using TwitchLib.Api.Core.Common;
using vscci.src.Data;
using System;
using System.Timers;

using Vintagestory.API.Server;

namespace vscci.src.CCIIntegrations.Twitch
{
    class TwitchAutherizationHelper
    {
        private HttpListener listener;
        private List<AuthScopes> authScopes;
        private string authForValidation;
        private Timer validationTimer;
        private ICoreServerAPI api;

        public event EventHandler<string> onAuthSucceful;
        public event EventHandler<string> onAuthFailed;
        public event EventHandler<string> onAuthBecameInvalid;

        public TwitchAutherizationHelper(ICoreServerAPI sapi)
        {
            api = sapi;
            listener = new HttpListener();
            validationTimer = new Timer();
            validationTimer.Elapsed += OnValidationTimer;
            validationTimer.AutoReset = true;

            authScopes = new List<AuthScopes>{
                AuthScopes.Channel_Feed_Read, AuthScopes.Helix_Channel_Read_Hype_Train,
                AuthScopes.Helix_Channel_Read_Subscriptions, AuthScopes.Helix_Bits_Read , AuthScopes.Helix_Channel_Read_Redemptions};
        }
         
        public string StartAuthFlow()
        {
            string scopeStr = null;
            foreach (var scope in authScopes)
                if (scopeStr == null)
                    scopeStr = Helpers.AuthScopesToString(scope);
                else
                    scopeStr += $"+{Helpers.AuthScopesToString(scope)}";

            string url = Constants.TWITCH_ID_URL + $"client_id={Constants.TWITCH_CLIENT_ID}&redirect_uri={Constants.TWITCH_REDIRECT_URI}&response_type=token&scope={scopeStr}";

            listener.Prefixes.Add(Constants.LISTEN_PREFIX);
            listener.Start();

            listener.BeginGetContext(OnReceiveAuthInfo, this); ;

            return url;
        }

        public bool ValidateToken(string token)
        {
            WebClient client = new WebClient();
            client.Headers.Add("Authorization", $"OAuth {token}");

            try
            {
                var result = client.DownloadString(Constants.TWITCH_VALIDATE_URL);
                api.Logger.Log(Vintagestory.API.Common.EnumLogType.Debug, "Token Valid With Response " + result);
                return result != null;
            }
            catch (WebException exception)
            {
                // bad response means token is probably bad
                api.Logger.Log(Vintagestory.API.Common.EnumLogType.Debug, "Dowload failed with exception " + exception.Message);
            }

            return false;
        }

        public void BeginValidationPingForToken(string token, double miliseconds)
        {
            if(validationTimer.Enabled)
            {
                validationTimer.Stop();
            }

            authForValidation = token;
            validationTimer.Interval = miliseconds;
            validationTimer.Start();
        }

        public void EndValidationPing()
        {
            if (validationTimer.Enabled)
            {
                validationTimer.Stop();
            }

            authForValidation = null;
        }

        private void OnValidationTimer(object source, ElapsedEventArgs e)
        {
            WebClient client = new WebClient();
            client.Headers.Add("Authorization", $"OAuth {authForValidation}");

            try
            {
                var result = client.DownloadString(Constants.TWITCH_VALIDATE_URL);
                api.Logger.Log(Vintagestory.API.Common.EnumLogType.Debug, "Token Valid With Response " + result);

            }
            catch (WebException exception)
            {
                // bad response means token is probably bad
                api.Logger.Log(Vintagestory.API.Common.EnumLogType.Debug, "Dowload failed with exception " + exception.Message);
                EndValidationPing();
                onAuthBecameInvalid?.Invoke(this, authForValidation);
            }
        }

        void OnReceiveAuthInfo(IAsyncResult result)
        {
            var context = listener.EndGetContext(result);
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            if (request.Url.AbsolutePath == "/implicit")
            {
                string responseString = "<html><head>\n"+
                                        "<script>\n" +
                                        "function onLoadFunction() {\n" +
                                        "\tvar hash = document.location.hash.substring(1);\n" +
                                        "\twindow.location.href = \"http://localhost:4444/rdr?\" + hash;\n" +
                                        //"\talert(\"hello\");"+
                                        "}\n" +
                                        "</script>\n"+
                                        "</head>\n"+
                                        "<body onload=\"onLoadFunction()\">\n"+
                                        "</body></html>";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                response.AddHeader("Cache-Control", "no-store, must-revalidate");
                response.AddHeader("Pragma", "no-cache");
                response.AddHeader("Expires", "0");
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
                listener.BeginGetContext(OnReceiveAuthInfo, this);
            }
            else if (request.Url.AbsolutePath == "/rdr")
            {
                string authCode = request.QueryString.Get("access_token");

                string responseString;

                if (authCode != null)
                {
                    responseString = "<html><script>setTimeout(window.close, 5000);</script><body>Auth Code Found !\nThis Page will Close automatically after 5 seconds.</body></html>";
                    onAuthSucceful?.Invoke(this, authCode);
                }
                else
                {
                    responseString = "<html><script>setTimeout(window.close, 5000);</script><body>Auth Code Not Found !\nThis Page will Close automatically after 5 seconds.</body></html>";
                    onAuthFailed?.Invoke(this, "Failed to Receive Auth Token On Redirect");
                }

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                response.AddHeader("Cache-Control", "no-store, must-revalidate");
                response.AddHeader("Pragma", "no-cache");
                response.AddHeader("Expires", "0");

                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();

                listener.Stop();
            }
            else
            {
                response.Close();
                listener.BeginGetContext(OnReceiveAuthInfo, this);
            }
        }
    }
}
