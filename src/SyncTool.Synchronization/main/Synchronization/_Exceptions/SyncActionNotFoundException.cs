using System;

namespace SyncTool.Synchronization
{
    public class SyncActionNotFoundException : Exception
    {
        public SyncActionNotFoundException(string message) : base(message)
        {
        }
    }
}