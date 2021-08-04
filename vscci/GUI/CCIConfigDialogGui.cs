namespace VSCCI.GUI
{
    using Vintagestory.API.Client;
    using VSCCI.Data;
    using VSCCI.CCINetworkTypes;
    using VSCCI.CCIIntegrations;
    using VSCCI.GUI.Elements;
    using System.Collections.Generic;

    public class CCIConfigDialogGui : GuiDialog
    {
        internal static readonly string[] Platforms = { "Streamlabs", "Streamelements" };

        public override string ToggleKeyCombinationCode => "ccigui";

        public override bool ShouldReceiveMouseEvents() => true;

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
        private int selectedPlatform;
        private ElementBounds streamPlatformTextInputBounds;
        private GuiTinyDropdown dropdown;

        public CCIConfigDialogGui(ICoreClientAPI capi) : base(capi)
        {
            this.capi = capi;
            userName = "None";
            id = "None";
            status = "Disconnected";
            slStatus = "Disconnected";
            serverStatus = "None-Allowed";
            selectedPlatform = 0;
        }

        public override void OnOwnPlayerDataReceived()
        {
            base.OnOwnPlayerDataReceived();
            
            var dialogBounds = ElementBounds.Fixed(EnumDialogArea.CenterFixed, 0, 0, 350, 250)
                .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);
            var bgBounds = dialogBounds.CopyOffsetedSibling(0, 0).WithFixedPadding(GuiStyle.DialogToScreenPadding);
            bgBounds.verticalSizing = ElementSizing.FitToChildren;

            var elementStart                  = ElementBounds.Fixed(25, 0);
            var twitchUserNameBounds          = elementStart.CopyOffsetedSibling(0,   45,  120, 50);
            var twitchUserNameBoundsDyn       = elementStart.CopyOffsetedSibling(120, 45,  120, 50);
            var twitchUserIdBounds            = elementStart.CopyOffsetedSibling(00,  60,  120, 50);
            var twitchUserIdBoundsDyn         = elementStart.CopyOffsetedSibling(120, 60,  120, 50);
            var twitchStatusBounds            = elementStart.CopyOffsetedSibling(0,   75,  120, 50);
            var twitchStatusBoundsDyn         = elementStart.CopyOffsetedSibling(120, 75,  120, 50);
            var streamPlatformTypeBounds      = elementStart.CopyOffsetedSibling(0,   90,  120, 50);
            var streamPlatformDropdownBounds  = elementStart.CopyOffsetedSibling(120, 90,  120, 15);
            var streamPlatformStatusBounds    = elementStart.CopyOffsetedSibling(0,   105, 120, 50);
            var streamPlatformStatusBoundsDyn = elementStart.CopyOffsetedSibling(120, 105, 120, 50);
            var streamPlatformKeyBounds       = elementStart.CopyOffsetedSibling(0,   120, 120, 50);
            streamPlatformTextInputBounds     = elementStart.CopyOffsetedSibling(120, 120, 120, 15);
            var serverStatusBounds            = elementStart.CopyOffsetedSibling(0,   135, 120, 50);
            var serverStatusBoundsDyn         = elementStart.CopyOffsetedSibling(120, 135, 120, 50);
            var connectToTwitchButtonBounds   = elementStart.CopyOffsetedSibling(0,   170, 100, 25);
            var loginToTwitchButtonBounds     = elementStart.CopyOffsetedSibling(0,   200, 100, 25);

            bgBounds.WithChildren(elementStart, twitchUserNameBounds, twitchUserNameBoundsDyn, twitchUserIdBounds, twitchUserIdBoundsDyn,
                twitchStatusBounds, twitchStatusBoundsDyn, streamPlatformTypeBounds, streamPlatformDropdownBounds, streamPlatformKeyBounds, streamPlatformStatusBounds, streamPlatformStatusBoundsDyn, streamPlatformTextInputBounds, serverStatusBoundsDyn, serverStatusBounds, serverStatusBoundsDyn, connectToTwitchButtonBounds, loginToTwitchButtonBounds);

            SingleComposer = capi.Gui.CreateCompo("cciconfig", dialogBounds)
                .AddDialogTitleBar("Content Creator Integration Config", () => TryClose(), CairoFont.WhiteSmallText())
                .AddDialogBG(bgBounds)
                .AddStaticText("Twitch User Name: ", CairoFont.WhiteSmallText().WithFontSize(10), twitchUserNameBounds)
                .AddDynamicText(userName, CairoFont.WhiteSmallText().WithFontSize(10), EnumTextOrientation.Left, twitchUserNameBoundsDyn, TWITCH_USER_KEY)
                .AddStaticText("Twitch ID: ", CairoFont.WhiteSmallText().WithFontSize(10), twitchUserIdBounds)
                .AddDynamicText(id, CairoFont.WhiteSmallText().WithFontSize(10), EnumTextOrientation.Left, twitchUserIdBoundsDyn, TWITCH_ID_KEY)
                .AddStaticText("Twitch Status: ", CairoFont.WhiteSmallText().WithFontSize(10), twitchStatusBounds)
                .AddDynamicText(status, CairoFont.WhiteSmallText().WithFontSize(10), EnumTextOrientation.Left, twitchStatusBoundsDyn, TWITCH_STATUS_KEY)
                .AddStaticText("Stream Platform Type: ", CairoFont.WhiteSmallText().WithFontSize(10), streamPlatformTypeBounds)
                .AddTinyDropDown(Platforms, Platforms, selectedPlatform, SelectionChangedDelegate, streamPlatformDropdownBounds, CairoFont.WhiteSmallText().WithFontSize(10), "platform_dropdown")
                .AddStaticText("Stream Platform Key: ", CairoFont.WhiteSmallText().WithFontSize(10), streamPlatformKeyBounds)
                .AddTextInput(streamPlatformTextInputBounds, null, CairoFont.WhiteSmallText().WithFontSize(10), STREAMLABS_TEXT_INPUT)
                .AddStaticText("Stream Platform Status: ", CairoFont.WhiteSmallText().WithFontSize(10), streamPlatformStatusBounds)
                .AddDynamicText(slStatus, CairoFont.WhiteSmallText().WithFontSize(10), EnumTextOrientation.Left, streamPlatformStatusBoundsDyn, STREAMLABS_STATUS_KEY)
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

            dropdown = SingleComposer.GetTinyDropDown("platform_dropdown");
        }

        public override void OnMouseDown(MouseEvent args)
        {
            base.OnMouseDown(args);

            dropdown?.OnMouseDown(capi, args);

            if (args.Handled) return;

            if (streamPlatformTextInputBounds.PositionInside(args.X, args.Y) != null)
            {
                SingleComposer.GetTextInput(STREAMLABS_TEXT_INPUT).OnFocusGained();
            }
        }

        public override void OnMouseMove(MouseEvent args)
        {
            base.OnMouseMove(args);

            dropdown?.OnMouseMove(capi, args);
        }

        public override void OnMouseUp(MouseEvent args)
        {
            base.OnMouseUp(args);

            dropdown?.OnMouseUp(capi, args);
        }

        public void UpdateGuiTwitchLoginText(string username, string userid)
        {
            userName = username;
            id = userid;

            if (userid != "None" && userid.Length > 4)
            {
                var stub = userid.Substring(userid.Length - 5, 4);
                id = userid.Remove(0, userid.Length - 4).Insert(0, new string('*', userid.Length - 4));
            }

            if (SingleComposer != null)
            {
                capi.Event.EnqueueMainThreadTask(() =>
                {
                    SingleComposer.GetDynamicText(TWITCH_USER_KEY).SetNewTextAsync(userName);
                    SingleComposer.GetDynamicText(TWITCH_ID_KEY).SetNewTextAsync(id);
                }, "VSCCI Update Login Text");
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

        private void SelectionChangedDelegate(string code, bool selected)
        {
            if (code == Platforms[0] && selected)
            {
                selectedPlatform = 0;
            }
            else if (selected)
            {
                selectedPlatform = 1;
            }
            else
            {
                capi.Logger.Error("VSCCI CCIConfigDialogGui Invalid State for Selection Changed");
            }
            capi.Event.EnqueueMainThreadTask(() =>
            {
                capi.ShowChatMessage($"code: {code}, selected: {selected}");
            }, "");
        }

        public override bool TryOpen()
        {
            if (base.TryOpen())
            {
                if (SingleComposer == null)
                {
                    OnOwnPlayerDataReceived();
                }
                return true;
            }
            return false;
        }

        public override bool TryClose()
        {
            if (base.TryClose())
            {
                return true;
            }
            return false;
        }
    }
}
