using System;
using System.Runtime.Serialization;

namespace SyncTool.Common.Groups
{
    [Serializable]
    public class GroupManagerException : Exception
    {
        public GroupManagerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public GroupManagerException(string message) : base(message)
        {
        }

        public GroupManagerException()
        {
        }

        protected GroupManagerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}