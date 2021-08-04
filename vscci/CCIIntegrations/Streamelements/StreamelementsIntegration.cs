namespace VSCCI.CCIIntegrations.Streamelements
{
    using VSCCI.Data;
    using VSCCI.CCINetworkTypes;
    using Vintagestory.API.Client;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using Vintagestory.API.Common;
    using StreamElementsNET;
    using SuperSocket.ClientEngine;
    using System;

    class StreamelementsIntegration : CCIIntegrationBase
    {
        private static string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyIjoiNjEwOWFmZmI4YTgzNDEzNjEwNTkwZDgxIiwicm9sZSI6Im93bmVyIiwiY2hhbm5lbCI6IjYxMDlhZmZiOGE4MzQxNjVhODU5MGQ4MiIsInByb3ZpZGVyIjoidHdpdGNoIiwiYXV0aFRva2VuIjoiQlY5dmlBaVZrMk5BNHlJYjREd3NBS1NHWFk1c2pBcnB6VWJMNDN0Y09hWXplVkk4IiwiaWF0IjoxNjI4MDI0ODI4LCJpc3MiOiJTdHJlYW1FbGVtZW50cyJ9.CDD0zZH5oksqL-BA_y42dQlGZ74QgETd9xbq4EGwXd8";

        private readonly ICoreClientAPI api;
        private Client seClient;
        private string jwtToken;
        private bool connected;

        public override bool IsConnected() => connected;

        public StreamelementsIntegration(ICoreClientAPI capi)
        {
            api = capi;
            connected = false;
            jwtToken = token;
        }

        public override void Connect()
        {
            if (seClient == null)
            {
                CreateSocket();
            }

            if (connected == false)
                seClient.Connect();
        }

        public override void Disconnect()
        {
            seClient?.Disconnect();
        }

        public override void Reset()
        {
            if (connected)
            {
                seClient?.Disconnect();
            }
            connected = false;
            seClient = null;
        }

        public override string GetAuthDataForSaving()
        {
            return jwtToken;
        }

        public override void SetRawAuthData(string authData)
        {
            if (jwtToken != authData)
            {
                jwtToken = authData;

                // reset socket if we changed auth token
                if (connected)
                {
                    seClient?.Disconnect();
                    seClient = null;
                }
            }
        }

        public override void SetAuthDataFromSaveData(string savedAuth)
        {
            jwtToken = savedAuth;

            // create / re-create the socket to update the token info
            // connect automaitcally from save

            CreateSocket();
            Connect();
        }

        public override CCIType GetCCIType()
        {
            return CCIType.Streamelements;
        }


        private void CreateSocket()
        {
            if (seClient != null)
            {
                if (connected)
                {
                    seClient.Disconnect();
                }
                seClient = null;
            }

            seClient = new Client(jwtToken);

            seClient.OnError += OnError;
            seClient.OnConnected += OnConnected;
            seClient.OnDisconnected += OnDisconnected;
            seClient.OnAuthenticated += OnAuthenticated;
            seClient.OnAuthenticationFailure += OnAuthenticationFailure;

            //seClient.OnReceivedRawMessage += OnReceivedRawMessage;

            seClient.OnFollower += OnFollow;
            seClient.OnFollowerLatest += OnFollowerLatest;
            seClient.OnHost += OnHost;
            seClient.OnHostLatest += OnHostLatest;
            seClient.OnSubscriber += OnSubscriber;
            seClient.OnSubscriberGiftedLatest += OnSubscriberGiftedLatest;
            seClient.OnSubscriberLatest += OnSubscriberLatest;
            seClient.OnCheer += OnCheer;
            seClient.OnCheerLatest += OnCheerLatest;
            seClient.OnTip += OnTip;
            seClient.OnTipLatest += OnTipLatest;
            seClient.OnRaid += OnRaid;
            seClient.OnRaidLatest += OnRaidLatest;
            seClient.OnRedemptionLatest += OnRedemptionLatest;
        }

        private void OnRedemptionLatest(object sender, StreamElementsNET.Models.Redemption.RedemptionLatest e)
        {
            api.Event.PushEvent(Constants.EVENT_REDEMPTION, new ProtoDataTypeAttribute<PointRedemptionData>(new PointRedemptionData()
            {
                who = e.Name,
                message = e.Message,
                redemptionName = e.Item,
                redemptionID = e.ItemId
            }));
        }

        private void OnRaidLatest(object sender, StreamElementsNET.Models.Raid.RaidLatest e)
        {
            api.Event.PushEvent(Constants.EVENT_RAID, new ProtoDataTypeAttribute<RaidData>(new RaidData()
            {
                raidChannel = e.Name,
                numberOfViewers = e.Amount
            }));
        }

        private void OnRaid(object sender, StreamElementsNET.Models.Raid.Raid e)
        {
            api.Event.PushEvent(Constants.EVENT_RAID, new ProtoDataTypeAttribute<RaidData>(new RaidData()
            {
                raidChannel = e.DisplayName,
                numberOfViewers = e.Amount
            }));
        }

        private void OnReceivedRawMessage(object sender, string e)
        {
            System.IO.FileMode mode = System.IO.File.Exists("eventData.log")
                ? System.IO.FileMode.Append : System.IO.FileMode.Create;
            using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.Open("eventData.log", mode, System.IO.FileAccess.Write)))
            {
                writer.Write(e);
            }
        }

        private void OnTipLatest(object sender, StreamElementsNET.Models.Tip.TipLatest e)
        {
            api.Event.PushEvent(Constants.EVENT_DONATION, new ProtoDataTypeAttribute<DonationData>(new DonationData()
            {
                who = e.Name,
                amount = (float)e.Amount,
                message = e.Message
            }));
        }

        private void OnTip(object sender, StreamElementsNET.Models.Tip.Tip e)
        {
            api.Event.PushEvent(Constants.EVENT_DONATION, new ProtoDataTypeAttribute<DonationData>(new DonationData()
            {
                who = e.Username,
                amount = (float)e.Amount,
                message = e.Message
            }));
        }

        private void OnCheerLatest(object sender, StreamElementsNET.Models.Cheer.CheerLatest e)
        {
            api.Event.PushEvent(Constants.EVENT_BITS_RECIEVED, new ProtoDataTypeAttribute<BitsData>(new BitsData()
            {
                from = e.Name,
                amount = e.Amount,
                message = e.Message
            }));
        }

        private void OnCheer(object sender, StreamElementsNET.Models.Cheer.Cheer e)
        {
            api.Event.PushEvent(Constants.EVENT_BITS_RECIEVED, new ProtoDataTypeAttribute<BitsData>(new BitsData()
            {
                from = e.DisplayName,
                amount = e.Amount,
                message = e.Message
            }));
        }

        private void OnSubscriberLatest(object sender, StreamElementsNET.Models.Subscriber.SubscriberLatest e)
        {
            api.Event.PushEvent(Constants.EVENT_NEW_SUB, new ProtoDataTypeAttribute<NewSubData>(new NewSubData()
            {
                message = e.Message,
                to = e.Name,
                from = e.Sender,
                isGift = false,
                months = e.Amount
            }));
        }

        private void OnSubscriberGiftedLatest(object sender, StreamElementsNET.Models.Subscriber.SubscriberGiftedLatest e)
        {
            api.Event.PushEvent(Constants.EVENT_NEW_SUB, new ProtoDataTypeAttribute<NewSubData>(new NewSubData()
            {
                message = e.Message,
                to = e.Name,
                from = e.Sender,
                isGift = true,
                months = e.Amount
            }));
        }

        private void OnSubscriber(object sender, StreamElementsNET.Models.Subscriber.Subscriber e)
        {
            api.Event.PushEvent(Constants.EVENT_NEW_SUB, new ProtoDataTypeAttribute<NewSubData>(new NewSubData()
            {
                message = e.Message,
                to = e.DisplayName,
                from = e.Sender,
                isGift = e.Gifted,
                months = e.Amount
            }));
        }

        private void OnHostLatest(object sender, StreamElementsNET.Models.Host.HostLatest e)
        {
            api.Event.PushEvent(Constants.EVENT_HOST, new ProtoDataTypeAttribute<HostData>(new HostData()
            {
                who = e.Name,
                viewers = e.Amount
            }));
        }

        private void OnHost(object sender, StreamElementsNET.Models.Host.Host e)
        {
            api.Event.PushEvent(Constants.EVENT_HOST, new ProtoDataTypeAttribute<HostData>(new HostData()
            {
                who = e.DisplayName,
                viewers = e.Amount
            }));
        }

        private void OnFollowerLatest(object sender, string e)
        {
            api.Event.PushEvent(Constants.EVENT_FOLLOW, new ProtoDataTypeAttribute<FollowData>(new FollowData()
            {
                who = e,
                channel = ""
            }));
        }

        private void OnFollow(object sender, StreamElementsNET.Models.Follower.Follower e)
        {
            api.Event.PushEvent(Constants.EVENT_FOLLOW, new ProtoDataTypeAttribute<FollowData>(new FollowData()
            {
                who = e.Username,
                channel = ""
            }));
        }

        private void OnConnected(object sender, EventArgs e)
        {
            api.Logger.Notification("Streamelements Socket Connected");
            api.Event.EnqueueMainThreadTask(() => api.ShowChatMessage("Streamelements Socket Connected"), null);
            
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            api.Logger.Notification("Streamelements Socket Disconnected");
            api.Event.EnqueueMainThreadTask(() => api.ShowChatMessage("Streamelements Socket Disconnected"), null);

            api.Event.PushEvent(Constants.CCI_EVENT_CONNECT_UPDATE,
                new ProtoDataTypeAttribute<CCIConnectionUpdate>(new CCIConnectionUpdate()
                {
                    type = CCIType.Streamelements,
                    status = "Disconnected"
                }));
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            api.Logger.Error("Streamelements Socket Error: {0}", e.Exception.Message);
            api.Event.EnqueueMainThreadTask(() => api.ShowChatMessage($"Streamelements Socket Error: {e.Exception.Message}"), null);
            connected = false;

            api.Event.PushEvent(Constants.CCI_EVENT_CONNECT_UPDATE, 
                new ProtoDataTypeAttribute<CCIConnectionUpdate>(new CCIConnectionUpdate() 
                { 
                    type = CCIType.Streamelements, 
                    status = "Error" 
                }));
            CallLoginError(new OnAuthFailedArgs() { Message = e.Exception.Message });
        }

        private void OnAuthenticationFailure(object sender, EventArgs e)
        {
            api.Logger.Error("Streamelements Socket Error: {0}", e);
            api.Event.EnqueueMainThreadTask(() => api.ShowChatMessage($"Streamelements Socket Error: {e}"), null);
            connected = false;

            api.Event.PushEvent(Constants.CCI_EVENT_CONNECT_UPDATE,
                new ProtoDataTypeAttribute<CCIConnectionUpdate>(new CCIConnectionUpdate()
                {
                    type = CCIType.Streamelements,
                    status = "Error"
                }));
            CallLoginError(new OnAuthFailedArgs() { Message = e.ToString()});
        }

        private void OnAuthenticated(object sender, StreamElementsNET.Models.Internal.Authenticated e)
        {
            api.Logger.Notification("Streamelements Socket Authenticated");
            api.Event.EnqueueMainThreadTask(() => api.ShowChatMessage($"Streamelements Socket Authenticated"), null);
            connected = true;

            CallLoginSuccess(jwtToken);
            CallConnectSuccess();
            api.Event.PushEvent(Constants.CCI_EVENT_CONNECT_UPDATE,
            new ProtoDataTypeAttribute<CCIConnectionUpdate>(new CCIConnectionUpdate()
            {
                type = CCIType.Streamelements,
                status = "Connected"
            }));
        }
    }
}
