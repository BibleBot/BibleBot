namespace BibleBot.Backend.Models
{
    public class DatabaseSettings : IDatabaseSettings
    {
        public string UserCollectionName { get; set; }
        public string GuildCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IDatabaseSettings
    {
        string UserCollectionName { get; set; }
        string GuildCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}