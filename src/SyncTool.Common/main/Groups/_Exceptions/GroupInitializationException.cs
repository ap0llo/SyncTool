using System;
using System.Runtime.Serialization;

namespace SyncTool.Common.Groups
{
    [Serializable]
    public class GroupInitializationException : Exception
    {
        public GroupInitializationException()
        {
        }

        public GroupInitializationException(string message) : base(message)
        {
        }

        public GroupInitializationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected GroupInitializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
