namespace VSCCI.CCIIntegrations
{
    using System;


    public enum CCIType
    {
        Twitch,
        Streamlabs,
        Streamelements
    }

    public class OnConnectFailedArgs : EventArgs
    {
        public string Reason { get; set; }

        public OnConnectFailedArgs() { }
    }

    public class OnAuthFailedArgs : EventArgs
    {
        public string Message { get; set; }

        public OnAuthFailedArgs() { }
    }

    public abstract class CCIIntegrationBase
    {
        public event EventHandler<OnAuthFailedArgs> OnLoginError;
        public event EventHandler<string> OnLoginSuccess;

        public event EventHandler OnConnectSuccess;
        public event EventHandler<OnConnectFailedArgs> OnConnectFailed;

        public abstract void SetRawAuthData(string authData);
        public abstract void SetAuthDataFromSaveData(string savedAuth);
        public abstract string GetAuthDataForSaving();
        public abstract void Connect();
        public abstract void Disconnect();
        public abstract void Reset();

        public abstract CCIType GetCCIType();
        protected virtual void CallLoginError(OnAuthFailedArgs args)
        {
            OnLoginError?.Invoke(this, args);
        }

        protected void CallLoginSuccess(string token)
        {
            OnLoginSuccess?.Invoke(this, token);
        }

        protected void CallConnectSuccess()
        {
            OnConnectSuccess?.Invoke(this, null);
        }

        protected void CallConnectFailed(OnConnectFailedArgs args)
        {
            OnConnectFailed?.Invoke(this, args);
        }
    }
}
