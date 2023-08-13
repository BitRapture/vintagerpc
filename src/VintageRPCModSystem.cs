using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

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

        ICoreClientAPI clientAPI;
        VintageRPC rpc;
        VintageRPCActivity activity;

        long rpcTickListener;
        long activityListener;

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);

            clientAPI = api;

            api.Event.RegisterRenderer(rpc, EnumRenderStage.Before);

            activityListener = api.World.RegisterGameTickListener(_ => activity.UpdateActivity(rpc, api), VintageRPCActivity.ActivityTickTime);
            rpcTickListener = api.World.RegisterGameTickListener(_ => rpc.TickRPC(), VintageRPC.RPCTickTime);
            
            api.Event.LeaveWorld += DisposeRPC;
        }

        void DisposeRPC()
        {
            clientAPI.World.UnregisterCallback(activityListener);
            clientAPI.World.UnregisterCallback(rpcTickListener);
            rpc.Dispose();
        }

        public override void Dispose()
        {
            base.Dispose();
            rpc.Dispose();
        }

        public VintageRPCModSystem()
        {
            rpc = new VintageRPC();
            activity = new VintageRPCActivity();
        }
    }
}
