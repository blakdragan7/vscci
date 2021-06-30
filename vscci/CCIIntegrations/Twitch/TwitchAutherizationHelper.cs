namespace vscci.CCIIntegrations.Twitch
{
    using System.Collections.Generic;
    using TwitchLib.Api.Core.Enums;
    using TwitchLib.Api.Core.Common;

    using vscci.Data;
    using vscci.CCINetworkTypes;

    using System;
    using System.Net;
    using System.Timers;

    using Vintagestory.API.Server;

    class TwitchAutherizationHelper
    {
        private List<AuthScopes>    authScopes;
        private string              authForValidation;
        private Timer               validationTimer;
        private ICoreServerAPI      api;
        IServerPlayer               player;

        public event EventHandler<string> OnAuthSucceful;
        public event EventHandler<string> OnAuthFailed;
        public event EventHandler<string> OnAuthBecameInvalid;

        public TwitchAutherizationHelper(ICoreServerAPI sapi)
        {
            api = sapi;

            validationTimer = new Timer();
            validationTimer.Elapsed += OnValidationTimer;
            validationTimer.AutoReset = true;

            authScopes = new List<AuthScopes>{
                AuthScopes.Channel_Feed_Read, AuthScopes.Helix_Channel_Read_Hype_Train,
                AuthScopes.Helix_Channel_Read_Subscriptions, AuthScopes.Helix_Bits_Read , AuthScopes.Helix_Channel_Read_Redemptions};

            api.Network.GetChannel(Constants.NETWORK_CHANNEL).SetMessageHandler<TwitchLoginStepResponse>(OnAuthResponse);
        }
         
        public void StartAuthFlow(IServerPlayer player)
        {
            this.player = player;

            string scopeStr = null;
            foreach (var scope in authScopes)
                if (scopeStr == null)
                    scopeStr = Helpers.AuthScopesToString(scope);
                else
                    scopeStr += $"+{Helpers.AuthScopesToString(scope)}";

            string url = Constants.TWITCH_ID_URL + $"client_id={Constants.TWITCH_CLIENT_ID}&redirect_uri={Constants.TWITCH_REDIRECT_URI}&response_type=token&scope={scopeStr}";

            api.Network.GetChannel(Constants.NETWORK_CHANNEL).SendPacket(new TwitchLoginStep() { url = url }, new[] { player });
        }

        private void OnAuthResponse(IServerPlayer fromPlayer, TwitchLoginStepResponse response)
        {
            if(this.player == fromPlayer)
            {
                if(response.success)
                {
                    OnAuthSucceful?.Invoke(this, response.authToken);
                }
                else
                {
                    OnAuthFailed?.Invoke(this, response.error);
                }
            }

            // just ignore if not meant for us
        }

        public bool ValidateToken(string token)
        {
            WebClient client = new WebClient();
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
            WebClient client = new WebClient();
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

        
    }
}
