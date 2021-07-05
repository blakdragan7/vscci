namespace vscci.CCIIntegrations.Twitch
{
    using System.Collections.Generic;
    using TwitchLib.Api.Core.Enums;
    using TwitchLib.Api.Core.Common;

    using vscci.Data;

    using System;
    using System.Net;
    using System.Timers;

    using Vintagestory.API.Client;
    using Vintagestory.API.Util;

    public class TwitchAutherizationHelper
    {
        private readonly List<AuthScopes> authScopes;
        private string authForValidation;
        private readonly Timer validationTimer;
        private ICoreClientAPI api;

        private readonly HttpListener listener;

        public event EventHandler<string> OnAuthSucceful;
        public event EventHandler<string> OnAuthFailed;
        public event EventHandler<string> OnAuthBecameInvalid;

        public TwitchAutherizationHelper(ICoreClientAPI sapi)
        {
            api = sapi;

            listener = new HttpListener();
            listener.Prefixes.Add(Constants.LISTEN_PREFIX);

            validationTimer = new Timer();
            validationTimer.Elapsed += OnValidationTimer;
            validationTimer.AutoReset = true;

            authScopes = new List<AuthScopes>{
                AuthScopes.Channel_Feed_Read, AuthScopes.Helix_Channel_Read_Hype_Train,
                AuthScopes.Helix_Channel_Read_Subscriptions, AuthScopes.Helix_Bits_Read , AuthScopes.Helix_Channel_Read_Redemptions};
        }

        public void StartAuthFlow()
        {
            string scopeStr = null;
            foreach (var scope in authScopes)
            {
                if (scopeStr == null)
                {
                    scopeStr = Helpers.AuthScopesToString(scope);
                }
                else
                {
                    scopeStr += $"+{Helpers.AuthScopesToString(scope)}";
                }
            }

            var url = Constants.TWITCH_ID_URL + $"client_id={Constants.TWITCH_CLIENT_ID}&redirect_uri={Constants.TWITCH_REDIRECT_URI}&response_type=token&scope={scopeStr}";
            NetUtil.OpenUrlInBrowser(url);

            listener.Start();
            listener.BeginGetContext(OnReceiveAuthInfo, this);
        }

        public bool ValidateToken(string token)
        {
            var client = new WebClient();
            client.Headers.Add("Authorization", $"OAuth {token}");

            try
            {
                var result = client.DownloadString(Constants.TWITCH_VALIDATE_URL);
                api.Logger.Log(Vintagestory.API.Common.EnumLogType.Debug, "Token Valid With Response {0}", result);
                return result != null;
            }
            catch (WebException exception)
            {
                // bad response means token is probably bad
                api.Logger.Log(Vintagestory.API.Common.EnumLogType.Debug, "Dowload failed with exception {0}", exception.Message);
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
            var client = new WebClient();
            client.Headers.Add("Authorization", $"OAuth {authForValidation}");

            try
            {
                var result = client.DownloadString(Constants.TWITCH_VALIDATE_URL);
                api.Logger.Log(Vintagestory.API.Common.EnumLogType.Debug, "Token Valid With Response {0}", result);

            }
            catch (WebException exception)
            {
                // bad response means token is probably bad
                api.Logger.Log(Vintagestory.API.Common.EnumLogType.Debug, "Dowload failed with exception {0}", exception.Message);
                EndValidationPing();
                OnAuthBecameInvalid?.Invoke(this, authForValidation);
            }
        }

        private void OnReceiveAuthInfo(IAsyncResult result)
        {
            var context = listener.EndGetContext(result);
            var request = context.Request;
            var response = context.Response;

            if (request.Url.AbsolutePath == "/implicit")
            {
                var responseString = "<html><head>\n" +
                                        "<script>\n" +
                                        "function onLoadFunction() {\n" +
                                        "\tvar hash = document.location.hash.substring(1);\n" +
                                        "\twindow.location.href = \"http://localhost:4444/rdr?\" + hash;\n" +
                                        "}\n" +
                                        "</script>\n" +
                                        "</head>\n" +
                                        "<body onload=\"onLoadFunction()\">\n" +
                                        "</body></html>";
                var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                response.AddHeader("Cache-Control", "no-store, must-revalidate");
                response.AddHeader("Pragma", "no-cache");
                response.AddHeader("Expires", "0");
                var output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
                listener.BeginGetContext(OnReceiveAuthInfo, this);
            }
            else if (request.Url.AbsolutePath == "/rdr")
            {
                var authCode = request.QueryString.Get("access_token");

                string responseString;

                if (authCode != null)
                {
                    responseString = "<html><script>setTimeout(window.close, 5000);</script><body>Auth Code Found !\nThis Page will Close automatically after 5 seconds.</body></html>";
                    OnAuthSucceful?.Invoke(this, authCode);
                }
                else
                {
                    responseString = "<html><script>setTimeout(window.close, 5000);</script><body>Auth Code Not Found !\nThis Page will Close automatically after 5 seconds.</body></html>";
                    OnAuthFailed?.Invoke(this, "Failed to Receive Auth Token On Redirect");
                }

                var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                response.AddHeader("Cache-Control", "no-store, must-revalidate");
                response.AddHeader("Pragma", "no-cache");
                response.AddHeader("Expires", "0");

                var output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();

                listener.Stop();
            }
            else
            {
                response.StatusCode = 404;
                response.Close();
                listener.BeginGetContext(OnReceiveAuthInfo, this);
            }
        }
    }
}
