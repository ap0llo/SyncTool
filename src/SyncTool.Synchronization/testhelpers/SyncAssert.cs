using System;
using System.Linq;
using Xunit;
using SyncTool.FileSystem.Versioning;
using SyncTool.Synchronization.State;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization.TestHelpers
{
    public static class SyncAssert
    {
        public static void ActionExists(ISyncStateService syncStateService, string path, int? expectedCount = null, ChangeType? expectedChangeType = null)
        {
            var actions = syncStateService.Actions.Where(a => StringComparer.OrdinalIgnoreCase.Equals(a.Path, path)).ToArray();
            Assert.NotEmpty(actions);

            if(expectedCount.HasValue)
            {
                Assert.Equal(expectedCount.Value, actions.Length);
            }

            if(expectedChangeType.HasValue)
            {
                foreach(var action in actions)
                {
                    Assert.Equal(expectedChangeType, action.InferChangeType());
                }
            }
        }

        public static void ToVersionMatches(IFile file, SyncAction action)
        {
            Assert.True(action.ToVersion.Matches(file));
        }

    }
}
