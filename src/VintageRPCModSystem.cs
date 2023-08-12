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

        const int activityTickTime = 5500;
        ICoreClientAPI clientAPI;
        VintageRPC rpc;

        string[] unicodeMoonPhases;
        string[] unicodeSeasons;
        string unicodeDay;
        string unicodeDeaths;
        string unicodeCalendar;

        long rpcCallbackListener;
        long rpcTickListener;

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);

            clientAPI = api;

            api.World.RegisterGameTickListener(UpdateActivity, activityTickTime);
            rpcCallbackListener = api.World.RegisterGameTickListener(rpc.TickCallbacks, VintageRPC.CallbackTickTime);
            rpcTickListener = api.World.RegisterGameTickListener(rpc.TickRPC, VintageRPC.RPCTickTime);
            api.Event.LeftWorld += DisposeRPC;
        }

        void DisposeRPC()
        {
            clientAPI.World.UnregisterCallback(rpcCallbackListener);
            clientAPI.World.UnregisterCallback(rpcTickListener);
            rpc.Dispose();
        }

        void UpdateActivity(float time = 0.0f)
        {
            if (rpc == null || !clientAPI.PlayerReadyFired)
                return;

            if (!rpc.IsRPCInstantiated)
            {
                rpc.InstantiateRPC(); 
                if (!rpc.IsRPCInstantiated)
                    return;
            }

            SetPlayerWorldActivityDetails();
            SetPlayerWorldActivityState();
            SetPlayerActivityState();
        }

        void SetPlayerActivityState()
        {
            var player = clientAPI.World.Player;

            var playerHealth = player.Entity.WatchedAttributes["health"] as ITreeAttribute;
            if (playerHealth == null)
                return;

            float maxHealth = playerHealth.GetFloat("maxhealth");
            float health = playerHealth.GetFloat("currenthealth");
            float healthT = float.Clamp((health / maxHealth) - 0.2f, 0.0f, 1.0f);
            int healthState = (int)float.Ceiling(rpc.StatusCount * healthT);
            health = float.Round(health, 2);
            maxHealth = float.Round(maxHealth, 2);
            
            string playerName = player.PlayerName;

            rpc.SetMiniStatus(healthState, $"{playerName} | {health}/{maxHealth} HP");
        }

        void SetPlayerWorldActivityState()
        {
            var calendar = clientAPI.World.Calendar;

            string timeOfDayEmoji = unicodeDay;
            int fullHourOfDay = calendar.FullHourOfDay;
            if (fullHourOfDay < 7 || fullHourOfDay > 19)
            {
                int phaseIndex = int.Clamp((int)calendar.MoonPhaseExact, 0, 8);
                timeOfDayEmoji = unicodeMoonPhases[phaseIndex];
            }

            var season = calendar.GetSeason(clientAPI.World.Player.Entity.Pos.AsBlockPos);
            string currentSeasonEmoji = unicodeSeasons[(int)season];

            int totalDeaths = clientAPI.World.Player.WorldData.Deaths;

            rpc.ActivityState = $"{timeOfDayEmoji} • {currentSeasonEmoji} • {unicodeDeaths}{totalDeaths} • {unicodeCalendar} {calendar.PrettyDate()}";
        }

        void SetPlayerWorldActivityDetails()
        {
            string playerType = clientAPI.IsSinglePlayer ? "singleplayer" : "multiplayer";
            string worldType = clientAPI.World.Player.WorldData.CurrentGameMode.ToString().ToLower();

            rpc.ActivityDetails = $"In a {playerType} {worldType} world";
        }

        public VintageRPCModSystem()
        {
            unicodeMoonPhases = new string[]
            {
               "🌑", "🌒", "🌓", "🌔", "🌕", "🌖", "🌗", "🌘", "🌙"
            };

            unicodeDay = "☀️";
            unicodeDeaths = "☠️";
            unicodeCalendar = "📅";

            unicodeSeasons = new string[]
            {
                "🌱", "🌴", "🍂", "❆"
            };

            rpc = new VintageRPC();
        }
    }
}
