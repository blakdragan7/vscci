namespace vscci.GUI
{
    using Vintagestory.API.Client;
    using vscci.Data;
    using vscci.CCINetworkTypes;
    using vscci.CCIIntegrations;
    using System.Collections.Generic;

    public class CCIConfigDialogGui : GuiDialog
    {
        public override string ToggleKeyCombinationCode => "ccigui";

        private const string TWITCH_USER_KEY = "twitch_user";
        private const string TWITCH_ID_KEY = "twitch_id";
        private const string TWITCH_STATUS_KEY = "twitch_status";
        private const string STREAMLABS_STATUS_KEY = "sl_status";
        private const string STREAMLABS_TEXT_INPUT = "sl_input";
        private const string SERVER_EVENT_STATUS_KEY = "server_status";
        private string userName;
        private string id;
        private string status;
        private string slStatus;
        private string serverStatus;
        private ElementBounds streamlabsTextInputBounds;

        public CCIConfigDialogGui(ICoreClientAPI capi) : base(capi)
        {
            this.capi = capi;
            userName = "None";
            id = "None";
            status = "Disconnected";
            slStatus = "Disconnected";
            serverStatus = "None-Allowed";
        }

        public override void OnOwnPlayerDataReceived()
        {
            base.OnOwnPlayerDataReceived();
            
            var dialogBounds = ElementBounds.Fixed(EnumDialogArea.CenterFixed, 0, 0, 350, 250)
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
            var streamlabsStatusBounds        = elementStart.CopyOffsetedSibling(0,   90,  100, 50);
            var streamlabsStatusBoundsDyn     = elementStart.CopyOffsetedSibling(100, 90,  100, 50);
            var streamlabsKeyBounds           = elementStart.CopyOffsetedSibling(0,   105, 100, 50);
                streamlabsTextInputBounds     = elementStart.CopyOffsetedSibling(100, 105, 100, 15);
            var serverStatusBounds            = elementStart.CopyOffsetedSibling(0,   120, 100, 50);
            var serverStatusBoundsDyn         = elementStart.CopyOffsetedSibling(100, 120, 100, 50);
            var connectToTwitchButtonBounds   = elementStart.CopyOffsetedSibling(0,   155, 100, 25);
            var loginToTwitchButtonBounds     = elementStart.CopyOffsetedSibling(0,   185, 100, 25);

            bgBounds.WithChildren(elementStart, twitchUserNameBounds, twitchUserNameBoundsDyn, twitchUserIdBounds, twitchUserIdBoundsDyn,
                twitchStatusBounds, twitchStatusBoundsDyn, streamlabsKeyBounds, streamlabsStatusBounds, streamlabsStatusBoundsDyn, streamlabsTextInputBounds, serverStatusBoundsDyn, serverStatusBounds, serverStatusBoundsDyn, connectToTwitchButtonBounds, loginToTwitchButtonBounds);

            SingleComposer = capi.Gui.CreateCompo("cciconfig", dialogBounds)
                .AddDialogTitleBar("Content Creator Integration Config", () => TryClose(), CairoFont.WhiteSmallText())
                .AddDialogBG(bgBounds)
                .AddStaticText("Twitch User Name: ", CairoFont.WhiteSmallText().WithFontSize(10), twitchUserNameBounds)
                .AddDynamicText(userName, CairoFont.WhiteSmallText().WithFontSize(10), EnumTextOrientation.Left, twitchUserNameBoundsDyn, TWITCH_USER_KEY)
                .AddStaticText("Twitch ID: ", CairoFont.WhiteSmallText().WithFontSize(10), twitchUserIdBounds)
                .AddDynamicText(id, CairoFont.WhiteSmallText().WithFontSize(10), EnumTextOrientation.Left, twitchUserIdBoundsDyn, TWITCH_ID_KEY)
                .AddStaticText("Twitch Status: ", CairoFont.WhiteSmallText().WithFontSize(10), twitchStatusBounds)
                .AddDynamicText(status, CairoFont.WhiteSmallText().WithFontSize(10), EnumTextOrientation.Left, twitchStatusBoundsDyn, TWITCH_STATUS_KEY)
                .AddStaticText("Streamlabs Key: ", CairoFont.WhiteSmallText().WithFontSize(10), streamlabsKeyBounds)
                .AddTextInput(streamlabsTextInputBounds, null, CairoFont.WhiteSmallText().WithFontSize(10), STREAMLABS_TEXT_INPUT)
                .AddStaticText("Streamlabs Status: ", CairoFont.WhiteSmallText().WithFontSize(10), streamlabsStatusBounds)
                .AddDynamicText(slStatus, CairoFont.WhiteSmallText().WithFontSize(10), EnumTextOrientation.Left, streamlabsStatusBoundsDyn, STREAMLABS_STATUS_KEY)
                .AddStaticText("Server Events: ", CairoFont.WhiteSmallText().WithFontSize(10), serverStatusBounds)
                .AddDynamicText(serverStatus, CairoFont.WhiteSmallText().WithFontSize(10), EnumTextOrientation.Left, serverStatusBoundsDyn, SERVER_EVENT_STATUS_KEY)
                .AddTextToggleButtons(new string[] { "Disconnect", "Login/Connect" }, CairoFont.ButtonText().WithFontSize(10),
                i =>
                {
                    switch (i)
                    {
                        case 0:
                            capi.Event.PushEvent(Constants.CCI_EVENT_DISCONNECT_REQUEST);
                            break;
                        case 1:
                            var slData = SingleComposer.GetTextInput(STREAMLABS_TEXT_INPUT).GetText();

                            var type = new List<CCIType>()
                            {
                                { CCIType.Twitch }
                            };

                            if(slData != null && slData != "")
                            {
                                type.Add(CCIType.Streamlabs);
                            }

                            capi.Event.PushEvent(Constants.CCI_EVENT_LOGIN_REQUEST,
                                new ProtoDataTypeAttribute<CCILoginRequest>(new CCILoginRequest()
                                {
                                    type = type,
                                    data = slData
                                }));
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

        public override void OnMouseDown(MouseEvent args)
        {
            base.OnMouseDown(args);

            if (streamlabsTextInputBounds.PositionInside(args.X, args.Y) != null)
            {
                SingleComposer.GetTextInput(STREAMLABS_TEXT_INPUT).OnFocusGained();
            }
        }

        public void UpdateGuiTwitchLoginText(string username, string userid)
        {
            userName = username;
            id = userid;

            if (SingleComposer != null)
            {
                SingleComposer.GetDynamicText(TWITCH_USER_KEY).SetNewTextAsync(username);
                SingleComposer.GetDynamicText(TWITCH_ID_KEY).SetNewTextAsync(userid);
            }
        }

        public void UpdateGuiTwitchConnectionText(string connection_status)
        {
            status = connection_status;
            if (SingleComposer != null)
            {
                SingleComposer.GetDynamicText(TWITCH_STATUS_KEY).SetNewTextAsync(connection_status);
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

        public void UpdateGuiStreamlabsConnectionText(string connection_status)
        {
            slStatus = connection_status;
            if (SingleComposer != null)
            {
                SingleComposer.GetDynamicText(STREAMLABS_STATUS_KEY).SetNewTextAsync(connection_status);
            }
        }

        public void UpdateGuiServerStatusText(string server_status)
        {
            serverStatus = server_status;
            if(SingleComposer != null)
            {
                SingleComposer.GetDynamicText(SERVER_EVENT_STATUS_KEY).SetNewTextAsync(serverStatus);
                SingleComposer.GetTextInput(STREAMLABS_TEXT_INPUT).SetValue("");
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
