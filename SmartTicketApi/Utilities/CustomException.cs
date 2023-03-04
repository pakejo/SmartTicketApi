using System.Runtime.Serialization;

namespace SmartTicketApi.Utilities
{
    [Serializable]
    public class CustomException : Exception
    {
        public CustomException() { }

        public CustomException(string message) : base(message) { }

        protected CustomException(SerializationInfo serializationInfo,
            StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }
    }
}
