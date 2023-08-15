using DiscordRPC;
using DiscordRPC.Message;
using System;
using Vintagestory.API.Client;

namespace VintageRPC.src
{
    internal class VintageRPC
    {
        public const float ActivityUpdateSeconds = 6.0f;
        public const string ClientID = "1139236686262439976";
        public bool IsInitialized { get; private set; }

        VintageRPCActivity activityData;
        DiscordRpcClient discordClient;
        RichPresence richPresenceData;

        ICoreClientAPI clientAPI;

        public void RegisterClientAPI(ICoreClientAPI api)
        {
            clientAPI = api;

            api.Event.RegisterGameTickListener(_ => SetActivity(), 50);
            api.Event.LeaveWorld += Reset;
        }

        public void Reset()
        {
            if (!IsInitialized)
                return;

            discordClient.ClearPresence();
        }

        public void Dispose()
        {
            if (!IsInitialized)
                return;
            discordClient.Dispose();
            IsInitialized = false;
        }

        void SetActivity()
        {
            if (!IsInitialized || !clientAPI.PlayerReadyFired)
                return;

            var playerState = activityData.GetPlayerActivityState(clientAPI);
            richPresenceData.Assets.SmallImageKey = playerState.Item1;
            richPresenceData.Assets.SmallImageText = playerState.Item2;
            richPresenceData.State = activityData.GetPlayerWorldActivityState(clientAPI);
            richPresenceData.Details = activityData.GetPlayerWorldActivityDetails(clientAPI);

            discordClient.SetPresence(richPresenceData);
        }

        void DiscordRPCError(object sender, ErrorMessage args)
        {
            if (clientAPI == null)
                Console.WriteLine(args.Message);
            else
                clientAPI.TriggerIngameError(sender, args.Code.ToString(), args.Message);
        }

        void SetupDiscordRPC()
        {
            activityData = new VintageRPCActivity();
            discordClient = new DiscordRpcClient(ClientID);
            discordClient.OnError += DiscordRPCError;
            IsInitialized = discordClient.Initialize();

            richPresenceData = new RichPresence()
            {
                Timestamps = Timestamps.Now,
                Assets = new Assets()
                {
                    LargeImageKey = activityData.DefaultLargeImage,
                    LargeImageText = activityData.ActivityName
                }
            };

            if (IsInitialized)
                discordClient.SetPresence(richPresenceData);
        }

        public VintageRPC()
        {
            SetupDiscordRPC();
        }

        ~VintageRPC()
        {
            Dispose();
        }
    }
}
