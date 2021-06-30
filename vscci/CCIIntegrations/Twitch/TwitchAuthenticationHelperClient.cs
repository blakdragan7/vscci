namespace vscci.CCIIntegrations.Twitch
{
    using System;
    using System.Net;

    using Vintagestory.API.Client;

    using vscci.Data;
    using vscci.CCINetworkTypes;

    class TwitchAuthenticationHelperClient
    {
        private HttpListener        listener;
        private ICoreClientAPI      api;

        public TwitchAuthenticationHelperClient(ICoreClientAPI capi)
        {
            api = capi;

            listener = new HttpListener();
            listener.Prefixes.Add(Constants.LISTEN_PREFIX);

            api.Network.GetChannel(Constants.NETWORK_CHANNEL).SetMessageHandler<TwitchLoginStep>(OnAuthStep);
        }

        private void OnAuthStep(TwitchLoginStep step)
        {
            listener.Start();
            listener.BeginGetContext(OnReceiveAuthInfo, this);

            api.ShowChatMessage("Login Step !");
            System.Diagnostics.Process.Start($"{step.url}");
        }

        void OnReceiveAuthInfo(IAsyncResult result)
        {
            var context = listener.EndGetContext(result);
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            if (request.Url.AbsolutePath == "/implicit")
            {
                string responseString = "<html><head>\n" +
                                        "<script>\n" +
                                        "function onLoadFunction() {\n" +
                                        "\tvar hash = document.location.hash.substring(1);\n" +
                                        "\twindow.location.href = \"http://localhost:4444/rdr?\" + hash;\n" +
                                        "}\n" +
                                        "</script>\n" +
                                        "</head>\n" +
                                        "<body onload=\"onLoadFunction()\">\n" +
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
                    api.Network.GetChannel(Constants.NETWORK_CHANNEL).SendPacket(new TwitchLoginStepResponse() { authToken=authCode, success=true });
                }
                else
                {
                    responseString = "<html><script>setTimeout(window.close, 5000);</script><body>Auth Code Not Found !\nThis Page will Close automatically after 5 seconds.</body></html>";
                    api.Network.GetChannel(Constants.NETWORK_CHANNEL).SendPacket(new TwitchLoginStepResponse() { error = "Failed to Receive Auth Token On Redirect", success=false });
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
                response.StatusCode = 404;
                response.Close();
                listener.BeginGetContext(OnReceiveAuthInfo, this);
            }
        }
    }
}
