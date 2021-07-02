namespace vscci.GUI
{
    using Vintagestory.API.Client;
    using vscci.CCINetworkTypes;
    using vscci.Data;

    class CCIConfigDialogGui : GuiDialog
    {
        public override string ToggleKeyCombinationCode => "ccigui";
        private string userName;
        private string id;
        private string status;

        public CCIConfigDialogGui(ICoreClientAPI capi) : base(capi)
        {
            this.capi = capi;
            userName = "None";
            id = "None";
            status = "Disconnected";
        }

        public override void OnOwnPlayerDataReceived()
        {
            base.OnOwnPlayerDataReceived();
            
            ElementBounds dialogBounds = ElementBounds.Fixed(EnumDialogArea.CenterFixed, 0, 0, 350, 200)
                .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);
            ElementBounds bgBounds = dialogBounds.CopyOffsetedSibling(0, 0).WithFixedPadding(GuiStyle.DialogToScreenPadding);
            bgBounds.verticalSizing = ElementSizing.FitToChildren;

            ElementBounds elementStart                  = ElementBounds.Fixed(25, 0);
            ElementBounds twitchUserNameBounds          = elementStart.CopyOffsetedSibling(0,   45, 100, 50);
            ElementBounds twitchUserNameBoundsDyn       = elementStart.CopyOffsetedSibling(100, 45, 100, 50);
            ElementBounds twitchUserIdBounds            = elementStart.CopyOffsetedSibling(00,  60, 100, 50);
            ElementBounds twitchUserIdBoundsDyn         = elementStart.CopyOffsetedSibling(100, 60, 100, 50);
            ElementBounds twitchStatusBounds            = elementStart.CopyOffsetedSibling(0,   75, 100, 50);
            ElementBounds twitchStatusBoundsDyn         = elementStart.CopyOffsetedSibling(100, 75, 100, 50);
            ElementBounds connectToTwitchButtonBounds   = elementStart.CopyOffsetedSibling(0,   110, 75, 25);
            ElementBounds loginToTwitchButtonBounds     = elementStart.CopyOffsetedSibling(0,   140, 75, 25);

            bgBounds.WithChildren(elementStart, twitchUserNameBounds, twitchUserNameBoundsDyn, twitchUserIdBounds, twitchUserIdBoundsDyn,
                twitchStatusBounds, twitchStatusBoundsDyn, connectToTwitchButtonBounds, loginToTwitchButtonBounds);

            SingleComposer = capi.Gui.CreateCompo("cciconfig", dialogBounds)
                .AddDialogTitleBar("Content Creator Integration Config", () => TryClose(), CairoFont.WhiteSmallText())
                .AddDialogBG(bgBounds)
                .AddStaticText("Twitch User Name: ", CairoFont.WhiteSmallText().WithFontSize(10), twitchUserNameBounds)
                .AddDynamicText(userName, CairoFont.WhiteSmallText().WithFontSize(10), EnumTextOrientation.Left, twitchUserNameBoundsDyn, "twitch_user")
                .AddStaticText("Twitch ID: ", CairoFont.WhiteSmallText().WithFontSize(10), twitchUserIdBounds)
                .AddDynamicText(id, CairoFont.WhiteSmallText().WithFontSize(10), EnumTextOrientation.Left, twitchUserIdBoundsDyn, "twitch_id")
                .AddStaticText("Twitch Status: ", CairoFont.WhiteSmallText().WithFontSize(10), twitchStatusBounds)
                .AddDynamicText(status, CairoFont.WhiteSmallText().WithFontSize(10), EnumTextOrientation.Left, twitchStatusBoundsDyn, "twitch_status")
                .AddTextToggleButtons(new string[] { "Connect", "Login"}, CairoFont.ButtonText().WithFontSize(10),
                i =>
                {
                    switch (i)
                    {
                        case 0:
                            capi.Network.GetChannel(Constants.NETWORK_CHANNEL).SendPacket(new CCIConnectRequest() {twitchid=""});
                            break;
                        case 1:
                            capi.Network.GetChannel(Constants.NETWORK_CHANNEL).SendPacket(new CCILoginRequest() { });
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
