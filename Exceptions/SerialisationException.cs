using System;
using System.Runtime.Serialization;

namespace Atlas.Exceptions
{
    [Serializable]
    public class SerialisationException : Exception
    {
        public SerialisationException() { }
        public SerialisationException(string message) : base(message) { }
        public SerialisationException(string message, Exception inner) : base(message, inner) { }
        protected SerialisationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
