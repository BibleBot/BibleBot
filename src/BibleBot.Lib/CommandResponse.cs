using System.Collections.Generic;

namespace BibleBot.Lib
{
    public class CommandResponse: IResponse
    {
        public bool OK { get; set; }
        public List<InternalEmbed> Pages { get; set; }
    }
}
