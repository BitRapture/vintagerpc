using System;
using System.Linq;
using System.Text;

namespace VintageRPC.src
{
    internal class VintageRPC
    {
        public const int RPCTickTime = 6000;
        public const int CallbackTickTime = 1000;
        public const string GameName = "Vintage Story";

        public bool IsRPCInstantiated => instanceResult == Discord.Result.Ok;
        public int StatusCount => miniStatus.Length - 1;

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

        public void TickCallbacks(float time = 0.0f)
        {
            if (IsRPCInstantiated)
            {
                discordRPC.RunCallbacks();

                if (discordRPC.CallbacksResult != Discord.Result.Ok)
                    Dispose();
            }
        }

        public void TickRPC(float time = 0.0f)
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

        byte[] EncodeUTF8(string text)
        {
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(text);
            return Enumerable.Concat(utf8Bytes, Enumerable.Repeat<byte>(0, 128 - utf8Bytes.Length)).ToArray();
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
