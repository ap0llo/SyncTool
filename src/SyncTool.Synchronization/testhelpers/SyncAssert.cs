using System.Linq;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;
using SyncTool.Synchronization.State;
using SyncTool.Synchronization.SyncActions;
using Xunit;

namespace SyncTool.Synchronization.TestHelpers
{
    public static class SyncAssert
    {    
        public static void Equal(ISyncPoint expected, ISyncPoint actual)
        {
            Assert.Equal(expected.Id, actual.Id);            
            Assert.Equal(expected.MultiFileSystemSnapshotId, actual.MultiFileSystemSnapshotId);            
        }
        

        public static void ActionsExist(ISyncActionService service, string path, SyncActionState? expectedState = null, int? expectedCount = null, ChangeType? expectedChangeType = null)
        {
            var actions = service[path].ToArray();

            Assert.NotEmpty(actions);

            if (expectedCount.HasValue)
            {
                Assert.Equal(expectedCount, actions.Length);
            }

            if (expectedState.HasValue)
            {                                   
                SyncAssert.IsInState(expectedState.Value, actions);
            }

            if (expectedChangeType != null)
            {
                SyncAssert.HasChangeType(expectedChangeType.Value, actions);
            }

        }

        public static void IsInState(SyncActionState expectedState, params SyncAction[] actions)
        {
            foreach (var action in actions)
            {
                Assert.Equal(expectedState, action.State);
            }
        }

        public static void HasChangeType(ChangeType type, params SyncAction[] objects)
        {
            foreach (var obj in objects)
            {
                Assert.Equal(type, obj.Type);
            }
        }

        public static void ToVersionMatches(IFile file, params SyncAction[] actions)
        {
            foreach (var action in actions)
            {
                Assert.True(action.ToVersion.Matches(file));
            }
        }

    }
}