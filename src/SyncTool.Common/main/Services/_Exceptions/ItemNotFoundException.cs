using System;
using System.Runtime.Serialization;

namespace SyncTool.Common.Services
{
    [Serializable]
    public class ItemNotFoundException : Exception
    {
        
        public ItemNotFoundException(string message) : base(message)
        {
        }

        public ItemNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ItemNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}