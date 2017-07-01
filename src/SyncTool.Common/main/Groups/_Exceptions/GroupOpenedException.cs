using System;
using System.Runtime.Serialization;

namespace SyncTool.Common.Groups
{
    public class GroupOpenedException : Exception
    {
        public GroupOpenedException()
        {
        }

        public GroupOpenedException(string message) : base(message)
        {
        }

        public GroupOpenedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected GroupOpenedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
