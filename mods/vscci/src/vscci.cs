using System;
using System.Collections.Generic;

using Vintagestory.API.Common;

using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;
using TwitchLib.Api.ThirdParty;
using TwitchLib.Api.Core.Enums;

using TwitchLib.Api.Events;

namespace vscci.src
{
    class vscci : ModSystem
    {
        private TwitchPubSub client;
        private ThirdParty utils;
        private IWorldAccessor world;
        private string TwitchID = "MischiefOfMice";
        //private const string ClientID = "izjwtydb6a3i11ftc5uewgc2gzjbow";
        //private const string RedirectURI = "http://localhost";
        private string AuthToken;

        private List<AuthScopes> authScopes;

        vscci()
        {
            authScopes = new List<AuthScopes>{
                AuthScopes.Channel_Feed_Read, AuthScopes.Helix_Channel_Read_Hype_Train,
                AuthScopes.Helix_Channel_Read_Subscriptions, AuthScopes.Helix_Bits_Read , AuthScopes.Helix_Channel_Read_Redemptions};

            utils = new ThirdParty(null, null, null);
            utils.AuthorizationFlow.OnUserAuthorizationDetected += onAuthSucceful;

            client = new TwitchPubSub();
            client.OnPubSubServiceConnected += onPubSubServiceConnected;
            client.OnListenResponse += onListenResponse;
            client.OnBitsReceived += onBitsReceived;
            client.OnFollow += onFollows;
            client.OnRaidGo += onRaid;
            client.OnChannelSubscription += onSubscription;
        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            //CreatedFlow flow = utils.AuthorizationFlow.CreateFlow("vscci", authScopes);

            //client.Connect();
            world = api.World;
        }

        private void onAuthSucceful(object sender, OnUserAuthorizationDetectedArgs args)
        {
            AuthToken = args.Token;
        }

        private void onBitsReceived(object sender, OnBitsReceivedArgs args)
        {
            if (args != null)
            {
                System.Console.WriteLine($"Twitch onBitsReceived {args}");
                world.Logger.Chat($"Twitch onBitsReceived {args}");
            }
        }

        private void onFollows(object sender, OnFollowArgs args)
        {
            if (args != null)
            {
                System.Console.WriteLine($"Twitch onFollows {args}");
                world.Logger.Chat($"Twitch onFollows {args}");
            }
        }

        private void onRaid(object sender, OnRaidGoArgs args)
        {
            if (args != null)
            {
                System.Console.WriteLine($"Twitch onRaid {args}");
                world.Logger.Chat($"Twitch onRaid {args}");
            }
        }

        private void onSubscription(object sender, OnChannelSubscriptionArgs args)
        {
            if (args != null)
            {
                System.Console.WriteLine($"Twitch onSubscription {args}");
                world.Logger.Chat($"Twitch onSubscription {args}");
            }
        }

        private void onPubSubServiceConnected(object sender, EventArgs e)
        {
            world.Logger.Chat($"Twitch CCI Connected");

            //client.ListenToBitsEvents(TwitchID);
            client.ListenToFollows(TwitchID);
            client.ListenToRaid(TwitchID);
            //client.ListenToSubscriptions(TwitchID);

            // SendTopics accepts an oauth optionally, which is necessary for some topics
            client.SendTopics(AuthToken);
        }

        private void onListenResponse(object sender, OnListenResponseArgs e)
        {
            if (e != null)
            {
                world.Logger.Chat($"onListenResponse:  was succeful {e.Successful} => wither error: {e.Response.Error}");
            }
        }
    }
}
