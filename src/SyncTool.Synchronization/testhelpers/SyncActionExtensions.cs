using SyncTool.FileSystem.Versioning;
using SyncTool.Synchronization.State;
using System;
using System.Collections.Generic;
using System.Text;

namespace SyncTool.Synchronization.TestHelpers
{
    public static class SyncActionExtensions
    {
        public static ChangeType InferChangeType(this SyncAction action)
        {
            if (action.FromVersion == null && action.ToVersion != null)
                return ChangeType.Added;
            else if (action.FromVersion != null && action.ToVersion == null)
                return ChangeType.Deleted;
            else if (action.FromVersion != null && action.ToVersion != null)
                return ChangeType.Modified;
            else
                throw new InvalidOperationException();        
        }

    }
}
