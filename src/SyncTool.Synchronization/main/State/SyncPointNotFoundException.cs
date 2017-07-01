using System;
using System.Runtime.Serialization;
using SyncTool.Common.Services;

namespace SyncTool.Synchronization.State
{
    [Serializable]
    public class SyncPointNotFoundException : ItemNotFoundException
    {

        public int SynchronizationStateId { get; }


        public SyncPointNotFoundException(int id) : base($"A SynchronizationState with id {id} could not be found")
        {
            SynchronizationStateId = id;
        }

        protected SyncPointNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}