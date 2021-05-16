using System.Collections.Generic;

namespace BibleBot.Lib
{
    public class VerseResponse
    {
        public bool OK { get; set; }
        public List<Verse> Verses { get; set; }
    }
}
