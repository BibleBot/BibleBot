using System.Collections.Generic;

using DSharpPlus.Entities;

namespace BibleBot.Lib
{
    public class CommandResponse
    {
        public bool OK { get; set; }
        public List<DiscordEmbed> Pages { get; set; }
    }
}
