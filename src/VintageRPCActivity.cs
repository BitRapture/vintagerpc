using Vintagestory.API.Client;
using Vintagestory.API.Datastructures;

namespace VintageRPC.src
{
    internal class VintageRPCActivity
    {
        public const int ActivityTickTime = 5500;
        public string DefaultSmallImage { get; private set; }
        public string DefaultLargeImage { get; private set; }
        public string ActivityName { get; private set; }
        public int SmallImages => smallImageStates.Length - 1;

        string[] smallImageStates;

        string[] unicodeMoonPhases;
        string[] unicodeSeasons;
        string unicodeDay;
        string unicodeDeaths;
        string unicodeCalendar;

        public (string, string) GetPlayerActivityState(ICoreClientAPI clientAPI)
        {
            var player = clientAPI.World.Player;

            var playerHealth = player.Entity.WatchedAttributes["health"] as ITreeAttribute;
            if (playerHealth == null)
                return ("", "");

            float maxHealth = playerHealth.GetFloat("maxhealth");
            float health = playerHealth.GetFloat("currenthealth");
            float healthT = float.Clamp((health / maxHealth) - 0.2f, 0.0f, 1.0f);
            int healthState = (int)float.Ceiling(SmallImages * healthT);
            health = float.Round(health, 2);
            maxHealth = float.Round(maxHealth, 2);

            string playerName = player.PlayerName;

            return (smallImageStates[healthState], $"{playerName} | {health}/{maxHealth} HP");
        }

        public string GetPlayerWorldActivityState(ICoreClientAPI clientAPI)
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

            return $"{timeOfDayEmoji} • {currentSeasonEmoji} • {unicodeDeaths}{totalDeaths} • {unicodeCalendar} {calendar.PrettyDate()}";
        }

        public string GetPlayerWorldActivityDetails(ICoreClientAPI clientAPI)
        {
            string playerType = clientAPI.IsSinglePlayer ? "singleplayer" : "multiplayer";
            string worldType = clientAPI.World.Player.WorldData.CurrentGameMode.ToString().ToLower();

            return $"In a {playerType} {worldType} world";
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

            smallImageStates = new string[]
{
                "vs_red",
                "vs_orange",
                "vs_green"
            };
            DefaultSmallImage = smallImageStates[0];

            DefaultLargeImage = "vs_logo";
            ActivityName = "Vintage Story";
        }
    }
}
