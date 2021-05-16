using System;

namespace BibleBot.Lib
{
    public class Request
    {
        public string UserId { get; set; }
        public string GuildId { get; set; }
        public string Token { get; set; }
        public string Body { get; set; }
    }
}
