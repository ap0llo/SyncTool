using System;
using System.Runtime.Serialization;

namespace SyncTool.Common.Groups
{
    [Serializable]
    public class GroupValidationException : Exception
    {
        public GroupValidationException()
        {
        }

        public GroupValidationException(string message) : base(message)
        {
        }

        public GroupValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected GroupValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
