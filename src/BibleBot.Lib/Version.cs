namespace BibleBot.Lib
{
    public class Version {
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public string Source { get; set; }

        public bool SupportsOldTestament { get; set; }
        public bool SupportsNewTestament { get; set; }
        public bool SupportsDeuterocanon { get; set; }
    }
}