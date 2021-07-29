namespace BibleBot.Backend.Models
{
    public interface IPreference
    {
        string Id { get; set; }
        string Version { get; set; }
        string Language { get; set; }
    }
}