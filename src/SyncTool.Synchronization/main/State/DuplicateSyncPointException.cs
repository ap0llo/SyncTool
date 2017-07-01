using System;
using System.Runtime.Serialization;
using SyncTool.Common.Services;

namespace SyncTool.Synchronization.State
{
    [Serializable]
    public class DuplicateSyncPointException : DuplicateItemException
    {

        public DuplicateSyncPointException(int id) : base($"A synchronization state with id {id} already exists")
        {
            
        }

        protected DuplicateSyncPointException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}