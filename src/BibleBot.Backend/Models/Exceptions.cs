namespace BibleBot.Backend.Models
{
    [System.Serializable]
    public class ProviderNotFoundException : System.Exception
    {
        public ProviderNotFoundException() { }
        public ProviderNotFoundException(string message) : base(message) { }
        public ProviderNotFoundException(string message, System.Exception inner) : base(message, inner) { }
        protected ProviderNotFoundException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}