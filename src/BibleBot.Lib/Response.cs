namespace BibleBot.Lib
{
    public interface IResponse
    {
        bool OK { get; set; }
        string LogStatement { get; set; }
    }
}