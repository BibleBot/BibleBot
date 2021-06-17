namespace BibleBot.Backend.Models
{
    public class DatabaseSettings : IDatabaseSettings
    {
        public string UserCollectionName { get; set; }
        public string GuildCollectionName { get; set; }
        public string VersionCollectionName { get; set; }
        public string LanguageCollectionName { get; set; }
        public string FrontendStatsCollectionName { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IDatabaseSettings
    {
        string UserCollectionName { get; set; }
        string GuildCollectionName { get; set; }
        string VersionCollectionName { get; set; }
        string LanguageCollectionName { get; set; }
        string FrontendStatsCollectionName { get; set; }
        string DatabaseName { get; set; }
    }
}