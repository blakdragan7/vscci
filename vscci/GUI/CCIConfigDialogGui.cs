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
        private const string PLATFORM_TEXT_INPUT = "sl_input";
        private const string SERVER_EVENT_STATUS_KEY = "server_status";
        private const string PLAFORM_CONENCT_BUTTON_KEY = "button-2";
        private const string PLAFORM_DISCONENCT_BUTTON_KEY = "button-3";
        private string userName;
        private string id;
        private string status;
        private string slStatus;
        private string serverStatus;
        private CCIType selectedPlatform;
        private int selectedPlatformIndex;
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
            selectedPlatform = CCIType.Streamlabs;
            selectedPlatformIndex = 0;
        }

        public override void OnOwnPlayerDataReceived()
        {
            base.OnOwnPlayerDataReceived();
            
            var dialogBounds = ElementBounds.Fixed(EnumDialogArea.CenterFixed, 0, 0, 350, 290)
                .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);
            var bgBounds = dialogBounds.CopyOffsetedSibling(0, 0).WithFixedPadding(GuiStyle.DialogToScreenPadding);
            bgBounds.verticalSizing = ElementSizing.FitToChildren;

            var elementStart                     = ElementBounds.Fixed(25, 0);
            var twitchUserNameBounds             = elementStart.CopyOffsetedSibling(0,   45,  120, 50);
            var twitchUserNameBoundsDyn          = elementStart.CopyOffsetedSibling(120, 45,  120, 50);
            var twitchUserIdBounds               = elementStart.CopyOffsetedSibling(00,  60,  120, 50);
            var twitchUserIdBoundsDyn            = elementStart.CopyOffsetedSibling(120, 60,  120, 50);
            var twitchStatusBounds               = elementStart.CopyOffsetedSibling(0,   75,  120, 50);
            var twitchStatusBoundsDyn            = elementStart.CopyOffsetedSibling(120, 75,  120, 50);
            var streamPlatformTypeBounds         = elementStart.CopyOffsetedSibling(0,   90,  120, 50);
            var streamPlatformDropdownBounds     = elementStart.CopyOffsetedSibling(120, 90,  120, 15);
            var streamPlatformStatusBounds       = elementStart.CopyOffsetedSibling(0,   105, 120, 50);
            var streamPlatformStatusBoundsDyn    = elementStart.CopyOffsetedSibling(120, 105, 120, 50);
            var streamPlatformKeyBounds          = elementStart.CopyOffsetedSibling(0,   120, 120, 50);
            streamPlatformTextInputBounds        = elementStart.CopyOffsetedSibling(120, 120, 120, 15);
            var serverStatusBounds               = elementStart.CopyOffsetedSibling(0,   135, 120, 50);
            var serverStatusBoundsDyn            = elementStart.CopyOffsetedSibling(120, 135, 120, 50);
            var connectToTwitchButtonBounds      = elementStart.CopyOffsetedSibling(0,   170, 100, 40);
            var disconnectToTwitchButtonBounds   = elementStart.CopyOffsetedSibling(120, 170, 100, 40);
            var connectToPlatformButtonBounds    = elementStart.CopyOffsetedSibling(0,   220, 100, 40);
            var disconnectToPlatformButtonBounds = elementStart.CopyOffsetedSibling(120, 220, 100, 40);

            bgBounds.WithChildren(elementStart, twitchUserNameBounds, twitchUserNameBoundsDyn, twitchUserIdBounds, twitchUserIdBoundsDyn,
                twitchStatusBounds, disconnectToTwitchButtonBounds, disconnectToPlatformButtonBounds, twitchStatusBoundsDyn, streamPlatformTypeBounds, streamPlatformDropdownBounds, streamPlatformKeyBounds, streamPlatformStatusBounds, streamPlatformStatusBoundsDyn, streamPlatformTextInputBounds, serverStatusBoundsDyn, serverStatusBounds, serverStatusBoundsDyn, connectToTwitchButtonBounds, connectToPlatformButtonBounds);

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
                .AddTinyDropDown(Platforms, Platforms, selectedPlatformIndex, EnumVerticalAlign.Top, EnumHorizontalAlign.Middle,PlatfformSelectionChanged, streamPlatformDropdownBounds, CairoFont.WhiteSmallText().WithFontSize(10), "platform_dropdown")
                .AddStaticText("Stream Platform Key: ", CairoFont.WhiteSmallText().WithFontSize(10), streamPlatformKeyBounds)
                .AddTextInput(streamPlatformTextInputBounds, null, CairoFont.WhiteSmallText().WithFontSize(10), PLATFORM_TEXT_INPUT)
                .AddStaticText("Stream Platform Status: ", CairoFont.WhiteSmallText().WithFontSize(10), streamPlatformStatusBounds)
                .AddDynamicText(slStatus, CairoFont.WhiteSmallText().WithFontSize(10), EnumTextOrientation.Left, streamPlatformStatusBoundsDyn, STREAMLABS_STATUS_KEY)
                .AddStaticText("Server Events: ", CairoFont.WhiteSmallText().WithFontSize(10), serverStatusBounds)
                .AddDynamicText(serverStatus, CairoFont.WhiteSmallText().WithFontSize(10), EnumTextOrientation.Left, serverStatusBoundsDyn, SERVER_EVENT_STATUS_KEY)
                .AddTextToggleButtons(new string[] { "Login Twitch", "Disconnect Twitch", "Login Platform", "Disconnect Platform" }, CairoFont.ButtonText().WithFontSize(10),
                i =>
                {
                    switch (i)
                    {
                        case 0:
                            capi.Event.PushEvent(Constants.CCI_EVENT_LOGIN_REQUEST,
                                new ProtoDataTypeAttribute<CCILoginRequest>(new CCILoginRequest()
                                {
                                    type = new List<CCIType>(){{ CCIType.Twitch }}
                                }));
                            break;
                        case 1:
                            capi.Event.PushEvent(Constants.CCI_EVENT_DISCONNECT_REQUEST,
                                new ProtoDataTypeAttribute<CCIDisconnectRequest>(new CCIDisconnectRequest()
                                {
                                    type = CCIType.Twitch
                                }));
                            break;
                        case 2:
                            var slData = SingleComposer.GetTextInput(PLATFORM_TEXT_INPUT).GetText();

                            if(slData.Length == 0)
                                break;

                            capi.Event.PushEvent(Constants.CCI_EVENT_LOGIN_REQUEST,
                                new ProtoDataTypeAttribute<CCILoginRequest>(new CCILoginRequest()
                                {
                                    type = new List<CCIType>() { { selectedPlatform } },
                                    data = slData
                                }));
                            break;
                        case 3:
                            capi.Event.PushEvent(Constants.CCI_EVENT_DISCONNECT_REQUEST,
                                new ProtoDataTypeAttribute<CCIDisconnectRequest>(new CCIDisconnectRequest()
                                {
                                    type = selectedPlatform
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
                    disconnectToTwitchButtonBounds,
                    connectToPlatformButtonBounds,
                    disconnectToPlatformButtonBounds,
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
                SingleComposer.GetTextInput(PLATFORM_TEXT_INPUT).OnFocusGained();
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
            }
        }

        public void UpdateGuiPlatformConnectionText(string connection_status)
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
                capi.Event.EnqueueMainThreadTask(() =>
                    SingleComposer.GetTextInput(PLATFORM_TEXT_INPUT).SetValue(""),
                "key update");
            }
        }

        public void UpdatePlatfformSelection(CCIType platform)
        {
            if (selectedPlatform != platform)
            {
                selectedPlatform = platform;
                selectedPlatformIndex = selectedPlatform == CCIType.Streamlabs ? 0 : 1;
                capi.Event.EnqueueMainThreadTask(()=>
                    dropdown?.SetSelectedIndex(selectedPlatformIndex),
                "dropdown selection update");
            }
        }

        private void PlatfformSelectionChanged(dynamic code, bool selected)
        {
            var strCode = code as string;
            var index = strCode == Platforms[0] ? 0 : 1;
            if (selectedPlatformIndex != index)
            {
                selectedPlatformIndex = index;
                selectedPlatform = index == 0 ? CCIType.Streamlabs : CCIType.Streamelements;
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
