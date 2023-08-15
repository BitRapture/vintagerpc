using System.Diagnostics;
using System.Threading;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

[assembly: ModInfo("VintageRPC", "vintagerpc",
    Authors = new string[] {"BitRapture"},
    Version = "1.0.0",
    Description = "Discord Rich Presence in Vintage Story",
    Side = "Client",
    RequiredOnClient = false,
    RequiredOnServer = false
    )]
[assembly: ModDependency("game", "1.18.8")]

namespace VintageRPC.src
{
    public class VintageRPCModSystem : ModSystem
    {
        public override bool AllowRuntimeReload => false;
        Thread thread;

        ICoreClientAPI clientAPI;

        public override void StartClientSide(ICoreClientAPI api)
        {
            api.Event.LeaveWorld += DisposeRPC;
            clientAPI = api;

            thread = new Thread(new ThreadStart(RunRPC));
            thread.IsBackground = true;
            thread.Start();

        }

        void RunRPC()
        {
            VintageRPC.NewInstance();

            Stopwatch stopWatch = Stopwatch.StartNew();
            while (VintageRPC.Instance != null)
            {
                VintageRPC.Instance.UpdateCallbacks();

                if (stopWatch.ElapsedMilliseconds / 1000.0f >= VintageRPC.RPCUpdateTime)
                {
                    stopWatch.Restart();
                    VintageRPC.Instance.UpdateRPCActivity(clientAPI);
                }
            }
        }

        void DisposeRPC()
        {
            VintageRPC.FreeInstance();
            thread.Join();
        }

        ~VintageRPCModSystem()
        {
            if (VintageRPC.Instance != null)
                DisposeRPC();
        }
    }
}
