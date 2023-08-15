using System;
using System.Text;
using Vintagestory.API.Client;

namespace VintageRPC.src
{
    internal class VintageRPC : IRenderer
    {
        public const int RPCTickTime = 10000;
        public const string GameName = "Vintage Story";

        public bool IsRPCInstantiated => instanceResult == Discord.Result.Ok;
        public int StatusCount => miniStatus.Length - 1;

        public int RenderRange
        {
            get { return 1; }
        }
        public double RenderOrder
        {
            get { return 0.0; }
        }

        public string ActivityDetails
        {
            get { return activityData.Details; }
            set { activityData.Details = value; }
        }
        public string ActivityState
        {
            get { return activityData.State.ToString(); }
            set { activityData.State = EncodeUTF8(value); }
        }

        string[] miniStatus;

        Discord.Discord discordRPC;
        Discord.Activity activityData;
        Discord.Result lastResult;
        Discord.Result instanceResult;

        public void TickCallbacks()
        {
            if (IsRPCInstantiated)
            {
                discordRPC.RunCallbacks();

                if (discordRPC.CallbacksResult != Discord.Result.Ok)
                    Dispose();
            }
        }

        public void TickRPC()
        {
            if (IsRPCInstantiated)
                discordRPC.GetActivityManager().UpdateActivity(activityData, result => result = lastResult);
        }

        public void ResetElapsedTime()
        {
            activityData.Timestamps.Start = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public void SetMiniStatus(int index, string text)
        {
            activityData.Assets.SmallImage = miniStatus[index];
            activityData.Assets.SmallText = text;
        }

        public void Dispose()
        {
            if (IsRPCInstantiated)
                discordRPC.Dispose();
            instanceResult = Discord.Result.NotRunning;
        }

        public void InstantiateRPC()
        {
            discordRPC = new Discord.Discord(activityData.ApplicationId, (ulong)Discord.CreateFlags.NoRequireDiscord);
            instanceResult = discordRPC.InstanceResult;
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            TickCallbacks();
        }

        byte[] EncodeUTF8(string text)
        {
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(text);
            byte[] payload = new byte[128];
            for (int i = 0; i < payload.Length; ++i)
                payload[i] = i < utf8Bytes.Length ? utf8Bytes[i] : (byte)0;
            return payload;
        }

        public VintageRPC()
        {
            miniStatus = new string[]
            {
                "vs_red",
                "vs_orange",
                "vs_green"
            };

            activityData = new Discord.Activity()
            {
                ApplicationId = 1139236686262439976,
                Name = GameName,
                Details = "",
                State = EncodeUTF8(""),
                Instance = false
            };
            activityData.Assets.LargeImage = "vs_logo";
            activityData.Assets.LargeText = GameName;
            ResetElapsedTime();

            InstantiateRPC();
        }

        ~VintageRPC()
        {
            Dispose();
        }
    }
}
