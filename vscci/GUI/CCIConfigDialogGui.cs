namespace vscci.GUI
{
    using Vintagestory.API.Client;
    using vscci.Data;

    public class CCIConfigDialogGui : GuiDialog
    {
        public override string ToggleKeyCombinationCode => "ccigui";
        private string userName;
        private string id;
        private string status;
        private string serverStatus;

        public CCIConfigDialogGui(ICoreClientAPI capi) : base(capi)
        {
            this.capi = capi;
            userName = "None";
            id = "None";
            status = "Disconnected";
            serverStatus = "None-Allowed";
        }

        public override void OnOwnPlayerDataReceived()
        {
            base.OnOwnPlayerDataReceived();
            
            var dialogBounds = ElementBounds.Fixed(EnumDialogArea.CenterFixed, 0, 0, 350, 200)
                .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);
            var bgBounds = dialogBounds.CopyOffsetedSibling(0, 0).WithFixedPadding(GuiStyle.DialogToScreenPadding);
            bgBounds.verticalSizing = ElementSizing.FitToChildren;

            var elementStart                  = ElementBounds.Fixed(25, 0);
            var twitchUserNameBounds          = elementStart.CopyOffsetedSibling(0,   45,  100, 50);
            var twitchUserNameBoundsDyn       = elementStart.CopyOffsetedSibling(100, 45,  100, 50);
            var twitchUserIdBounds            = elementStart.CopyOffsetedSibling(00,  60,  100, 50);
            var twitchUserIdBoundsDyn         = elementStart.CopyOffsetedSibling(100, 60,  100, 50);
            var twitchStatusBounds            = elementStart.CopyOffsetedSibling(0,   75,  100, 50);
            var twitchStatusBoundsDyn         = elementStart.CopyOffsetedSibling(100, 75,  100, 50);
            var serverStatusBounds            = elementStart.CopyOffsetedSibling(0,   90,  100, 50);
            var serverStatusBoundsDyn         = elementStart.CopyOffsetedSibling(100, 90,  100, 50);
            var connectToTwitchButtonBounds   = elementStart.CopyOffsetedSibling(0,   125, 100, 25);
            var loginToTwitchButtonBounds     = elementStart.CopyOffsetedSibling(0,   155, 100, 25);

            bgBounds.WithChildren(elementStart, twitchUserNameBounds, twitchUserNameBoundsDyn, twitchUserIdBounds, twitchUserIdBoundsDyn,
                twitchStatusBounds, twitchStatusBoundsDyn, serverStatusBounds, serverStatusBoundsDyn, connectToTwitchButtonBounds, loginToTwitchButtonBounds);

            SingleComposer = capi.Gui.CreateCompo("cciconfig", dialogBounds)
                .AddDialogTitleBar("Content Creator Integration Config", () => TryClose(), CairoFont.WhiteSmallText())
                .AddDialogBG(bgBounds)
                .AddStaticText("Twitch User Name: ", CairoFont.WhiteSmallText().WithFontSize(10), twitchUserNameBounds)
                .AddDynamicText(userName, CairoFont.WhiteSmallText().WithFontSize(10), EnumTextOrientation.Left, twitchUserNameBoundsDyn, "twitch_user")
                .AddStaticText("Twitch ID: ", CairoFont.WhiteSmallText().WithFontSize(10), twitchUserIdBounds)
                .AddDynamicText(id, CairoFont.WhiteSmallText().WithFontSize(10), EnumTextOrientation.Left, twitchUserIdBoundsDyn, "twitch_id")
                .AddStaticText("Twitch Status: ", CairoFont.WhiteSmallText().WithFontSize(10), twitchStatusBounds)
                .AddDynamicText(status, CairoFont.WhiteSmallText().WithFontSize(10), EnumTextOrientation.Left, twitchStatusBoundsDyn, "twitch_status")
                .AddStaticText("Server Events: ", CairoFont.WhiteSmallText().WithFontSize(10), serverStatusBounds)
                .AddDynamicText(serverStatus, CairoFont.WhiteSmallText().WithFontSize(10), EnumTextOrientation.Left, serverStatusBoundsDyn, "server_status")
                .AddTextToggleButtons(new string[] { "Disconnect", "Login/Connect" }, CairoFont.ButtonText().WithFontSize(10),
                i =>
                {
                    switch (i)
                    {
                        case 0:
                            capi.Event.PushEvent(Constants.CCI_EVENT_DISCONNECT_REQUEST);
                            break;
                        case 1:
                            capi.Event.PushEvent(Constants.CCI_EVENT_LOGIN_REQUEST);
                            break;
                        default:
                            break;
                    }
                    capi.Event.RegisterCallback(dt => SingleComposer.GetToggleButton("buttons-" + i).On = false, 50);
                },
                new ElementBounds[]
                {
                    connectToTwitchButtonBounds,
                    loginToTwitchButtonBounds,
                    dialogBounds
                }, "buttons")
                .Compose();

            SingleComposer.GetToggleButton("buttons-0").Enabled = false;
        }

        public void UpdateGuiLoginText(string username, string userid)
        {
            userName = username;
            id = userid;

            if (SingleComposer != null)
            {
                SingleComposer.GetDynamicText("twitch_user").SetNewTextAsync(username);
                SingleComposer.GetDynamicText("twitch_id").SetNewTextAsync(userid);
            }
        }

        public void UpdateGuiConnectionText(string connection_status)
        {
            status = connection_status;
            if (SingleComposer != null)
            {
                SingleComposer.GetDynamicText("twitch_status").SetNewTextAsync(connection_status);
                if(connection_status == "Connected")
                {
                    SingleComposer.GetToggleButton("buttons-0").Enabled = true;
                    SingleComposer.GetToggleButton("buttons-0").On = false;
                }
                else
                {
                    SingleComposer.GetToggleButton("buttons-0").Enabled = false;
                    SingleComposer.GetToggleButton("buttons-0").On = true;
                }
            }
        }

        public void UpdateGuiServerStatusText(string server_status)
        {
            serverStatus = server_status;
            if(SingleComposer != null)
            {
                SingleComposer.GetDynamicText("server_status").SetNewTextAsync(serverStatus);
            }
        }

        public override bool TryOpen()
        {
            if (base.TryOpen())
            {
                OnOwnPlayerDataReceived();
                return true;
            }
            return false;
        }

        public override bool TryClose()
        {
            if (base.TryClose())
            {
                Dispose();
                return true;
            }
            return false;
        }
    }
}
