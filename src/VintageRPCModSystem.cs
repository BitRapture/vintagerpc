using Vintagestory.API.Client;
using Vintagestory.API.Common;

[assembly: ModInfo("VintageRPC", "vintagerpc",
    Authors = new string[] {"BitRapture"},
    Version = "1.1.0",
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
        VintageRPC rpc;

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return rpc.IsInitialized && forSide == EnumAppSide.Client;
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            rpc.RegisterClientAPI(api);
        }

        public VintageRPCModSystem()
        {
            rpc = new VintageRPC();
        }
    }
}
