using System.Collections.Generic;
using System.Linq;
using Xunit;
using SyncTool.Sql.TestHelpers;
using SyncTool.Sql.Model;

namespace SyncTool.Sql.Test.Model
{
    public class SyncStateRepositoryTest : SqlTestBase
    {
        [Fact]
        public void New_database_contains_default_sync_state()
        {
            // ARRANGE
            var repository = new SyncStateRepository(Database);

            // ACT
            var syncState = repository.GetSyncState();
            repository.LoadActions(syncState);
            repository.LoadConflicts(syncState);

            // ASSERT
            Assert.Null(syncState.SnapshotId);
            Assert.Equal(1, syncState.Version);
            Assert.Empty(syncState.Actions);
            Assert.Empty(syncState.Conflicts);
        }

        [Fact]
        public void UpdateSyncState_saves_new_state()
        {
            // ARRANGE
            var repository = new SyncStateRepository(Database);
            var initialState = repository.GetSyncState();

            // ACT
            var newState = new SyncStateDo()
            {
                SnapshotId = "SnapshotId",
                Version = initialState.Version + 1,
                Actions = new List<SyncActionDo>(),
                Conflicts = new List<SyncConflictDo>()
            };

            repository.UpdateSyncState(newState);

            // ASSERT
            var updatedState = repository.GetSyncState();
            repository.LoadActions(updatedState);
            repository.LoadConflicts(updatedState);

            Assert.Equal("SnapshotId", updatedState.SnapshotId);
            Assert.Equal(initialState.Version + 1, updatedState.Version);
            Assert.Empty(updatedState.Actions);
            Assert.Empty(updatedState.Conflicts);
        }

        [Fact]
        public void UpdateSyncState_prevents_concurrent_writes()
        {
            // ARRANGE
            var repository = new SyncStateRepository(Database);
            var initialState = repository.GetSyncState();

            // ACT
            var update1 = new SyncStateDo()
            {
                SnapshotId = "SnapshotId",
                Version = initialState.Version + 1,
                Actions = new List<SyncActionDo>(),
                Conflicts = new List<SyncConflictDo>()
            };

            var update2 = new SyncStateDo()
            {
                SnapshotId = "AnotherSnapshotId",
                Version = initialState.Version + 1,
                Actions = new List<SyncActionDo>(),
                Conflicts = new List<SyncConflictDo>()
            };

            
            repository.UpdateSyncState(update2);
            Assert.Throws<DatabaseUpdateException>(() => repository.UpdateSyncState(update1));

            // ASSERT
            
            var updatedState = repository.GetSyncState();
            Assert.Equal("AnotherSnapshotId", updatedState.SnapshotId);
            Assert.Equal(initialState.Version + 1, updatedState.Version);            
        }

        [Fact]
        public void UpdateSyncState_saves_conflicts()
        {
            // ARRANGE
            var fileReference = new FileReferenceDo("/file1", 42, 23);
            var repository = new SyncStateRepository(Database);
            var initialState = repository.GetSyncState();

            // ACT
            var newState = new SyncStateDo()
            {
                SnapshotId = "SnapshotId",
                Version = initialState.Version + 1,
                Actions = new List<SyncActionDo>(),
                Conflicts = new List<SyncConflictDo>()
                {
                    new SyncConflictDo()
                    {
                        SnapshotId = "SomeValue",
                        ConflictingVersions = new List<FileReferenceDo>() { fileReference, null }
                    }
                }
            };

            repository.UpdateSyncState(newState);

            // ASSERT
            var updatedState = repository.GetSyncState();
            repository.LoadActions(updatedState);
            repository.LoadConflicts(updatedState);

            Assert.Equal("SnapshotId", updatedState.SnapshotId);
            Assert.Equal(initialState.Version + 1, updatedState.Version);
            Assert.Empty(updatedState.Actions);
            Assert.Single(updatedState.Conflicts);

            var conflict = updatedState.Conflicts.Single();
            Assert.Equal("SomeValue", conflict.SnapshotId);

            repository.LoadConflictingVersions(conflict);
            Assert.Equal(2, conflict.ConflictingVersions.Count);
            Assert.Single(conflict.ConflictingVersions.Where(x => x != null));
            Assert.Single(conflict.ConflictingVersions.Where(x => x == null));

            var nonNullFileReference = conflict.ConflictingVersions.Single(x => x != null);
            Assert.Equal(fileReference.LastWriteUnixTimeTicks, nonNullFileReference.LastWriteUnixTimeTicks);
            Assert.Equal(fileReference.Length, nonNullFileReference.Length);
            Assert.Equal(fileReference.Path, nonNullFileReference.Path);
        }

        [Fact]
        public void UpdateSyncState_saves_actions()
        {
            // ARRANGE
            var repository = new SyncStateRepository(Database);
            var initialState = repository.GetSyncState();

            // ACT
            var newState = new SyncStateDo()
            {
                Version = initialState.Version + 1,
                SnapshotId = "Value",
                Conflicts = new List<SyncConflictDo>(),
                Actions = new List<SyncActionDo>()
                {
                    new SyncActionDo() { FromVersion = null, ToVersion = new FileReferenceDo("file1", 23, 42) },
                    new SyncActionDo() { FromVersion =new FileReferenceDo("file2", 23, 42), ToVersion = new FileReferenceDo("file2", 13, 37) },
                    new SyncActionDo() { FromVersion =new FileReferenceDo("file3", 23, 42), ToVersion = null }
                }
            };
            repository.UpdateSyncState(newState);

            // ASSERT
            var state = repository.GetSyncState();
            repository.LoadActions(state);

            Assert.Equal(3, state.Actions.Count);
            foreach(var action in state.Actions)
            {
                repository.LoadVersions(action);
            }
            Assert.Single(state.Actions.Where(a => a.FromVersion == null && a.ToVersion != null));
            Assert.Single(state.Actions.Where(a => a.FromVersion != null && a.ToVersion != null));
            Assert.Single(state.Actions.Where(a => a.FromVersion != null && a.ToVersion != null));

        }

        [Fact]
        public void UpdateSyncState_deletes_previous_actions()
        {
            // ARRANGE
            var repository = new SyncStateRepository(Database);
            var initialState = repository.GetSyncState();

            var state1 = new SyncStateDo()
            {
                Version = initialState.Version + 1,
                SnapshotId = "Value",
                Conflicts = new List<SyncConflictDo>(),
                Actions = new List<SyncActionDo>()
                {
                    new SyncActionDo() { FromVersion = null, ToVersion = new FileReferenceDo("file1", 23, 42) },
                    new SyncActionDo() { FromVersion = new FileReferenceDo("file2", 23, 42), ToVersion = new FileReferenceDo("file2", 13, 37) },
                    new SyncActionDo() { FromVersion = new FileReferenceDo("file3", 23, 42), ToVersion = null }
                }
            };
            repository.UpdateSyncState(state1);


            // ACT
            var state2 = new SyncStateDo()
            {
                Version = state1.Version + 1,
                SnapshotId = "Value",
                Conflicts = new List<SyncConflictDo>(),
                Actions = new List<SyncActionDo>()
                {
                    new SyncActionDo() { FromVersion =new FileReferenceDo("file2", 23, 42), ToVersion = new FileReferenceDo("file2", 13, 37) },
                }
            };
            repository.UpdateSyncState(state2);


            // ASSERT
            var updatedState = repository.GetSyncState();
            repository.LoadActions(updatedState);

            Assert.Single(updatedState.Actions);
        }

        [Fact]
        public void UpdateSyncState_deletes_previous_conflicts()
        {
            // ARRANGE
            var fileReference = new FileReferenceDo("/file1", 42, 23);
            var repository = new SyncStateRepository(Database);
            var initialState = repository.GetSyncState();

            var state1 = new SyncStateDo()
            {
                SnapshotId = "SnapshotId",
                Version = initialState.Version + 1,
                Actions = new List<SyncActionDo>(),
                Conflicts = new List<SyncConflictDo>()
                {
                    new SyncConflictDo()
                    {
                        SnapshotId = "SomeValue",
                        ConflictingVersions = new List<FileReferenceDo>() { fileReference, null }
                    }
                }
            };
            repository.UpdateSyncState(state1);


            // ACT
            var state2 = new SyncStateDo()
            {
                SnapshotId = "SnapshotId",
                Version = state1.Version + 1,
                Actions = new List<SyncActionDo>(),
                Conflicts = new List<SyncConflictDo>()            
            };

            repository.UpdateSyncState(state2);
            

            // ASSERT
            var updatedState = repository.GetSyncState();
            repository.LoadConflicts(updatedState);

            Assert.Empty(updatedState.Conflicts);
        }
    }
}
