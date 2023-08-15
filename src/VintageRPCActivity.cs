using Vintagestory.API.Client;
using Vintagestory.API.Datastructures;

namespace VintageRPC.src
{
    internal class VintageRPCActivity
    {
        public const int ActivityTickTime = 5500;

        string[] unicodeMoonPhases;
        string[] unicodeSeasons;
        string unicodeDay;
        string unicodeDeaths;
        string unicodeCalendar;

        public void UpdateActivity(VintageRPC rpc, ICoreClientAPI clientAPI)
        {
            if (rpc == null || clientAPI == null || !clientAPI.PlayerReadyFired)
                return;
            
            //if (!rpc.IsRPCInstantiated)
            //{
            //    rpc.InstantiateRPC();
            //    if (!rpc.IsRPCInstantiated)
            //        return;
            //}

            SetPlayerWorldActivityDetails(rpc, clientAPI);
            SetPlayerWorldActivityState(rpc, clientAPI);
            SetPlayerActivityState(rpc, clientAPI);
        }

        void SetPlayerActivityState(VintageRPC rpc, ICoreClientAPI clientAPI)
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

        void SetPlayerWorldActivityState(VintageRPC rpc, ICoreClientAPI clientAPI)
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

        void SetPlayerWorldActivityDetails(VintageRPC rpc, ICoreClientAPI clientAPI)
        {
            string playerType = clientAPI.IsSinglePlayer ? "singleplayer" : "multiplayer";
            string worldType = clientAPI.World.Player.WorldData.CurrentGameMode.ToString().ToLower();

            rpc.ActivityDetails = $"In a {playerType} {worldType} world";
        }

        public VintageRPCActivity()
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
        }
    }
}
