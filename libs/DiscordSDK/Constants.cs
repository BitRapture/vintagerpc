using System;
using System.IO;

namespace Discord
{
    static class Constants
    {
#if PLATFORM_LINUX
        public const string DllName = ".rpc/discord_game_sdk";
#else 
        public const string DllName = ".rpc\\discord_game_sdk";
#endif
    }
}
