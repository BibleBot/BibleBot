using System.Collections.Generic;

namespace BibleBot.Lib
{
    public class CommandResponse
    {
        public bool OK { get; set; }
        public List<DiscordEmbed> Pages { get; set; }
    }
}
