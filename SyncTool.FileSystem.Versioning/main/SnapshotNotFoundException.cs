using System;

namespace SyncTool.FileSystem.Versioning
{
    public class SnapshotNotFoundException : Exception
    {

        public SnapshotNotFoundException(string id) : base($"Snapshot with id '{id}' not found")
        {
            
        }

    }
}