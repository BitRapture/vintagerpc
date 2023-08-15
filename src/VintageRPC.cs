using System;
using System.Text;
using System.Threading;
using Vintagestory.API.Client;

namespace VintageRPC.src
{
    internal class VintageRPC
    {
        public static VintageRPC Instance {  get; private set; }

        public const float RPCUpdateTime = 6.0f;
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

        VintageRPCActivity activity;
        Mutex mutex;

        #region Using_Discord_SDK
        public void UpdateCallbacks()
        {
            mutex.WaitOne();

            if (IsRPCInstantiated)
            {
                discordRPC.RunCallbacks();

                if (discordRPC.CallbacksResult != Discord.Result.Ok)
                    Dispose();
            }

            mutex.ReleaseMutex();
        }

        public void UpdateRPCActivity(ICoreClientAPI api)
        {
            mutex.WaitOne();

            if (IsRPCInstantiated)
            {
                activity.UpdateActivity(this, api);
                discordRPC.GetActivityManager().UpdateActivity(activityData, result => result = lastResult);
            }

            mutex.ReleaseMutex();
        }

        public void Dispose()
        {
            mutex.WaitOne();

            if (IsRPCInstantiated)
                discordRPC.Dispose();
            instanceResult = Discord.Result.NotRunning;

            mutex.ReleaseMutex();
        }

        public void InstantiateRPC()
        {
            mutex.WaitOne();

            discordRPC = new Discord.Discord(activityData.ApplicationId, (ulong)Discord.CreateFlags.NoRequireDiscord);
            instanceResult = discordRPC.InstanceResult;

            mutex.ReleaseMutex();
        }
        #endregion

        public void ResetElapsedTime()
        {
            activityData.Timestamps.Start = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public void SetMiniStatus(int index, string text)
        {
            activityData.Assets.SmallImage = miniStatus[index];
            activityData.Assets.SmallText = text;
        }

        byte[] EncodeUTF8(string text)
        {
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(text);
            byte[] payload = new byte[128];
            for (int i = 0; i < payload.Length; ++i)
                payload[i] = i < utf8Bytes.Length ? utf8Bytes[i] : (byte)0;
            return payload;
        }

        void SetupActivityData()
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
        }

        public static void NewInstance()
        {
            Instance = new VintageRPC();
        }

        public static void FreeInstance()
        {
            Instance = null;
        }

        public VintageRPC()
        {
            if (Instance == null)
            {
                Instance = this;
                mutex = new Mutex(true, "VintageRPCMutex");
                activity = new VintageRPCActivity();
                SetupActivityData();
                InstantiateRPC();
            }
        }

        ~VintageRPC()
        {
            if (Instance == this)
            {
                mutex.Dispose();
                Dispose();
                Instance = null;
            }
        }
    }
}
