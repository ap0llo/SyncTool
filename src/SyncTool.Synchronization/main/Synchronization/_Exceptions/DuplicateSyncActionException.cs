using System;

namespace SyncTool.Synchronization
{
    public class DuplicateSyncActionException : Exception
    {
        public DuplicateSyncActionException(string message) : base(message)
        {
        }
    }
}