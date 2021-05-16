using System.Collections.Generic;

namespace BibleBot.Lib
{
    public class CommandResponse
    {
        public bool OK { get; set; }
        public List<IDiscordEmbed> Pages { get; set; }
    }
}
