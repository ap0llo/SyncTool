using System;
using System.Runtime.Serialization;
using SyncTool.Common.Services;

namespace SyncTool.Configuration
{
    public class SyncFolderNotFoundException : ItemNotFoundException
    {
        public SyncFolderNotFoundException(string message) : base(message)
        {
        }

        public SyncFolderNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public SyncFolderNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}