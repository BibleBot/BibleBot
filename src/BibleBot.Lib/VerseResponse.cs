using System.Collections.Generic;

namespace BibleBot.Lib
{
    public class VerseResponse: IResponse
    {
        public bool OK { get; set; }
        public List<Verse> Verses { get; set; }
        public string DisplayStyle { get; set; }
    }
}
