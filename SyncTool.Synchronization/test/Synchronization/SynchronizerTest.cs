// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015-2016, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Moq;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;
using SyncTool.Synchronization.Conflicts;
using SyncTool.Synchronization.SyncActions;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.Synchronization
{
    /// <summary>
    /// Tests for <see cref="Synchronizer"/>
    /// </summary>
    public class SynchronizerTest
    {
        readonly Synchronizer m_Instance;

        public SynchronizerTest()
        {
            m_Instance = new Synchronizer(new FilePropertiesComparer());
        }



        [Fact(DisplayName = nameof(Synchronizer) + ".Synchronize(): Empty diffs yield empty result")]
        public void Synchronize_Empty_diffs_yield_empty_result()
        {
            var result = m_Instance.Synchronize(
                GetMockedFileSystemDiff(null, null, Array.Empty<IChange>()).Object,
                GetMockedFileSystemDiff(null, null, Array.Empty<IChange>()).Object);

            Assert.Empty(result.Actions);
            Assert.Empty(result.Conflicts);
        }

        [Fact(DisplayName = nameof(Synchronizer) + ".Synchronize(): Addition in one diff")]
        public void Synchronize_Addition_in_one_diff()
        {
            var rightDirectory = new Directory("root")
            {
                root => new EmptyFile(root, "file1")
            };

            var leftDirectory = new Directory("root")
            {
                root => new EmptyFile(root, "file2")
            };

            var leftChanges = new IChange[]
            {
                new Change(ChangeType.Added, null, leftDirectory.GetFile("file2"))
            };


            var result = m_Instance.Synchronize(
                GetMockedFileSystemDiff(null, leftDirectory, leftChanges).Object,
                GetMockedFileSystemDiff(rightDirectory, rightDirectory, Array.Empty<IChange>()).Object);
                

            Assert.Single(result.Actions);
            Assert.Empty(result.Conflicts);
            Assert.IsType<AddFileSyncAction>(result.Actions.Single());

            var syncAction = (AddFileSyncAction) result.Actions.Single();
            Assert.Equal(SyncParticipant.Right, syncAction.Target);
            Assert.Equal("file2", syncAction.NewFile.Name);
        }

        [Fact(DisplayName = nameof(Synchronizer) + ".Synchronize(): Modification in one diff")]
        public void Synchronize_Modification_in_one_diff()
        {
            var rightDirectory = new Directory("root")
            {
                root => new EmptyFile(root, "file1"),
                root => new EmptyFile(root, "file2") {LastWriteTime = DateTime.Parse("01.01.1980")}
            };

            var leftDirectory = new Directory("root")
            {
                root => new EmptyFile(root, "file2")
            };

            var leftChanges = new IChange[]
            {
                new Change(ChangeType.Modified, rightDirectory.GetFile("file2"), leftDirectory.GetFile("file2"))
            };


            var result = m_Instance.Synchronize(
                GetMockedFileSystemDiff(null, leftDirectory, leftChanges).Object,
                GetMockedFileSystemDiff(rightDirectory, rightDirectory, Array.Empty<IChange>()).Object);

            Assert.Single(result.Actions);
            Assert.Empty(result.Conflicts);
            Assert.IsType<ReplaceFileSyncAction>(result.Actions.Single());

            var syncAction = (ReplaceFileSyncAction) result.Actions.Single();
            Assert.Equal(SyncParticipant.Right, syncAction.Target);
        }

        [Fact(DisplayName = nameof(Synchronizer) + ".Synchronize(): Deletion in one diff")]
        public void Synchronize_Deletion_in_one_diff()
        {
            var rightDirectory = new Directory("root")
            {
                root => new EmptyFile(root, "file1"),
                root => new EmptyFile(root, "file2")
            };

            var leftDirectoryBeforeChange = new Directory("root")
            {
                root => new EmptyFile(root, "file2")
            };

            var leftChanges = new IChange[]
            {
                new Change(ChangeType.Deleted, leftDirectoryBeforeChange.GetFile("file2"), null)
            };


            var result = m_Instance.Synchronize(
                GetMockedFileSystemDiff(leftDirectoryBeforeChange, null, leftChanges).Object,
                GetMockedFileSystemDiff(rightDirectory, rightDirectory, Array.Empty<IChange>()).Object);
            
                
            Assert.Empty(result.Conflicts);

            Assert.Single(result.Actions);
            Assert.IsType<RemoveFileSyncAction>(result.Actions.Single());

            var syncAction = (RemoveFileSyncAction) result.Actions.Single();
            Assert.Equal(SyncParticipant.Right, syncAction.Target);
            Assert.Equal("file2", syncAction.RemovedFile.Name);
        }

        [Fact(DisplayName = nameof(Synchronizer) + ".Synchronize() yields empty result if change leads to already consistent state")]
        public void Synchronize_yields_empty_result_if_change_leads_to_already_consistent_state()
        {
            var rightDirectory = new Directory("root")
            {
                root => new EmptyFile(root, "file1"),
                root => new EmptyFile(root, "file2") {LastWriteTime = DateTime.Now}
            };

            var leftChanges = new IChange[]
            {
                new Change(ChangeType.Added, null, rightDirectory.GetFile("file2"))
            };


            var result = m_Instance.Synchronize(
                GetMockedFileSystemDiff(null, null, leftChanges).Object,
                GetMockedFileSystemDiff(rightDirectory, rightDirectory, Array.Empty<IChange>()).Object);
                
            Assert.Empty(result.Actions);
            Assert.Empty(result.Conflicts);
        }

        [Fact]
        public void Synchronize_Additions_in_both_diffs_but_for_different_files()
        {
            var leftDirectoryBefore = new Directory("root");
            var leftDirectoryAfter = new Directory("root")
            {
                root => new Directory(root, "dir1")
                {
                    dir1 => new EmptyFile(dir1, "file1")
                }
            };

            var rightDirectoryBefore = new Directory("root");
            var rightDirectoryAfter = new Directory("root")
            {
                root => new Directory(root, "dir2")
                {
                    dir2 => new EmptyFile(dir2, "file2")
                }
            };

            var leftChanges = new IChange[] {new Change(ChangeType.Added, null, leftDirectoryAfter.GetFile("dir1/file1"))};
            var rightChanges = new IChange[] {new Change(ChangeType.Added, null, rightDirectoryAfter.GetFile("dir2/file2"))};


            var syncResult = m_Instance.Synchronize(
                GetMockedFileSystemDiff(leftDirectoryBefore, leftDirectoryAfter, leftChanges).Object,
                GetMockedFileSystemDiff(rightDirectoryBefore, rightDirectoryAfter, rightChanges).Object);
                
            Assert.Empty(syncResult.Conflicts);
            Assert.Equal(2, syncResult.Actions.Count());

            var leftSyncAction = (AddFileSyncAction) syncResult.Actions.Single(action => action.Target == SyncParticipant.Left);
            var rightSyncAction = (AddFileSyncAction) syncResult.Actions.Single(action => action.Target == SyncParticipant.Right);

            Assert.Equal("dir2/file2", leftSyncAction.NewFile.Path);
            Assert.Equal("dir1/file1", rightSyncAction.NewFile.Path);
        }

        [Fact(DisplayName = nameof(Synchronizer) + ".Synchronize(): 'Addition and modification': Conflict")]
        public void Synchronize_Addition_and_modification_Conflict()
        {
            // add file to left directory
            var leftDirectoryBefore = new Directory("root");
            var leftDirectoryAfter = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = DateTime.Parse("01.01.1990") }
            };

            var leftChanges = new IChange[] { new Change(ChangeType.Added, null, leftDirectoryAfter.GetFile("file1")) };

            //the right directory contained the same file that was added on the left side before the modification
            var rightDirectoryBefore = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = DateTime.Parse("01.01.1991") }
            };
            var rightDirectoryAfter = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = DateTime.Parse("01.01.1992") }
            };

            var rightChanges = new IChange[]
            {
                new Change(ChangeType.Modified,
                           rightDirectoryBefore.GetFile("file1"),
                           rightDirectoryAfter.GetFile("file1"))
            };

            var syncResult = m_Instance.Synchronize(
                GetMockedFileSystemDiff(leftDirectoryBefore, leftDirectoryAfter, leftChanges).Object,
                GetMockedFileSystemDiff(rightDirectoryBefore, rightDirectoryAfter, rightChanges).Object);
             

            Assert.Empty(syncResult.Actions);
            Assert.Single(syncResult.Conflicts);
            Assert.IsType<MultipleVersionSyncConflict>(syncResult.Conflicts.Single());

            var action = (MultipleVersionSyncConflict) syncResult.Conflicts.Single();
            Assert.True(action.ConflictedFiles.All(f => f.Name == "file1"));
            Assert.True(action.ConflictedFiles.SingleOrDefault(f => f.LastWriteTime == rightDirectoryAfter.GetFile("file1").LastWriteTime) != null);
            Assert.True(action.ConflictedFiles.SingleOrDefault(f => f.LastWriteTime == leftDirectoryAfter.GetFile("file1").LastWriteTime) != null);        
        }

        [Fact(DisplayName = nameof(Synchronizer) + ".Synchronize(): 'Addition and Modification ': Modification of file added to the other directory")]
        public void Synchronize_Addition_and_Modification_Modification_of_file_added_to_the_other_directory()
        {
            // add file to left directory
            var leftDirectoryBefore = new Directory("root");
            var leftDirectoryAfter = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = DateTime.Parse("01.01.1990") }
            };

            var leftChanges = new IChange[] { new Change(ChangeType.Added, null, leftDirectoryAfter.GetFile("file1")) };

            //the right directory contained the same file that was added on the left side before the modification
            var rightDirectoryBefore = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = DateTime.Parse("01.01.1990") }
            };
            var rightDirectoryAfter = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = DateTime.Parse("01.01.1991") }
            };

            var rightChanges = new IChange[]
            {
                new Change(ChangeType.Modified,
                           rightDirectoryBefore.GetFile("file1"),
                           rightDirectoryAfter.GetFile("file1"))
            };

            var syncResult = m_Instance.Synchronize(
               GetMockedFileSystemDiff(leftDirectoryBefore, leftDirectoryAfter, leftChanges).Object,
               GetMockedFileSystemDiff(rightDirectoryBefore, rightDirectoryAfter, rightChanges).Object);
               
            // expected result: The file added on the left is the older version of the same file from the right directory
            // replace the file on the left with the new version from the right

            Assert.Empty(syncResult.Conflicts);
            Assert.Single(syncResult.Actions);


            var action = (ReplaceFileSyncAction) syncResult.Actions.Single();
            Assert.Equal(SyncParticipant.Left, action.Target);
            Assert.Equal("file1", action.OldVersion.Name);
            Assert.Equal(rightDirectoryBefore.GetFile("file1").LastWriteTime, action.OldVersion.LastWriteTime);
            Assert.Equal("file1", action.NewVersion.Name);
            Assert.Equal(rightDirectoryAfter.GetFile("file1").LastWriteTime, action.NewVersion.LastWriteTime);

        }

        [Fact(DisplayName = nameof(Synchronizer) + ".Synchronize(): Addition and Modification results in consistent state")]
        public void Synchronize_Addition_and_Modification_results_in_consistent_state()
        {
            // add file to left directory
            var leftDirectoryBefore = new Directory("root");
            var leftDirectoryAfter = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = DateTime.Now }
            };

            var leftChanges = new IChange[] {new Change(ChangeType.Added, null, leftDirectoryAfter.GetFile("file1"))};

            //modify file in right directory so it matches the file added in the left directory
            var rightDirectoryBefore = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = DateTime.Parse("01.01.1990") }
            };
            var rightDirectoryAfter = new Directory("root")
            {
                root => leftDirectoryAfter.GetFile("file1").WithParent(root)
            };

            var rightChanges = new IChange[]
            {
                new Change(ChangeType.Modified, 
                           rightDirectoryBefore.GetFile("file1"),
                           rightDirectoryAfter.GetFile("file1"))
            };

            var syncResult = m_Instance.Synchronize(
                GetMockedFileSystemDiff(leftDirectoryBefore, leftDirectoryAfter, leftChanges).Object,
                GetMockedFileSystemDiff(rightDirectoryBefore, rightDirectoryAfter, rightChanges).Object);

            Assert.Empty(syncResult.Actions);
            Assert.Empty(syncResult.Conflicts);

        }

        [Fact(DisplayName = nameof(Synchronizer) + ".Synchronize(): Double Addition results in consistent state")]
        public void Synchronize_Double_Addition_results_in_consistent_state()
        {
            var lastWriteTime = DateTime.Parse("01.01.1990");
            var leftChanges = new IChange[]
            {
                new Change(ChangeType.Added, null, FileMockingHelper.GetMockedFile("file", lastWriteTime, 23))
            };
            var rightChanges = new IChange[]
            {
                new Change(ChangeType.Added, null,FileMockingHelper.GetMockedFile("file", lastWriteTime, 23))
            };

            var syncResult = m_Instance.Synchronize(
                GetMockedFileSystemDiff(null, null, leftChanges).Object,
                GetMockedFileSystemDiff(null, null, rightChanges).Object);

            Assert.Empty(syncResult.Actions);
            Assert.Empty(syncResult.Conflicts);
        }

        [Fact(DisplayName = nameof(Synchronizer) + ".Synchronize(): Double Addition results in conflict")]
        public void Synchronize_Double_Addition_results_in_conflict()
        {
            var lastWriteTime = DateTime.Parse("01.01.1990");
            var leftChanges = new IChange[]
            {
                new Change(
                    ChangeType.Added, 
                    null, 
                    FileMockingHelper.GetFileMock()
                    .Named("file1")
                    .WithLastWriteTime(lastWriteTime)
                    .WithLength(23)
                    .WithParentNamed("dir1")
                    .Object)
            };
            var rightChanges = new IChange[]
            {
                new Change(
                    ChangeType.Added, 
                    null,                     
                    FileMockingHelper.GetFileMock()
                    .Named("file1")
                    .WithLastWriteTime(lastWriteTime.AddHours(1))
                    .WithLength(23)
                    .WithParentNamed("dir1")
                    .Object)
            };


            var syncResult = m_Instance.Synchronize(
                GetMockedFileSystemDiff(null, new Directory("root"), leftChanges).Object,
                GetMockedFileSystemDiff(null, new Directory("root"), rightChanges).Object);
                
            Assert.Empty(syncResult.Actions);
            Assert.Single(syncResult.Conflicts);
            var action = syncResult.Conflicts.Single();
            Assert.IsType<MultipleVersionSyncConflict>(action);
        }

        [Fact(DisplayName = nameof(Synchronizer) + ".Synchronize() Addition and Deletion throws " + nameof(InvalidOperationException))]
        public void Synchronize_Addition_and_Deletion_throws_InvalidOperationException()
        {
            var leftChanges = new IChange[] {new Change(ChangeType.Added, null, FileMockingHelper.GetMockedFile("file", DateTime.Now, 42))};
            var rightChanges = new IChange[] { new Change(ChangeType.Deleted, FileMockingHelper.GetMockedFile("file", DateTime.Now, 42), null) };

            var leftDiff = GetMockedFileSystemDiff(null, null, leftChanges).Object;
            var rightDiff = GetMockedFileSystemDiff(null, null, rightChanges).Object;

            Assert.Throws<InvalidOperationException>(() => m_Instance.Synchronize(leftDiff, rightDiff));
            Assert.Throws<InvalidOperationException>(() => m_Instance.Synchronize(rightDiff, leftDiff));
        }

        [Fact(DisplayName = nameof(Synchronizer) + ".Synchronize(): Modification and Deletion Conflict")]
        public void Synchronize_Modification_and_Deletion_Conflict()
        {
            // modification in left directory
            var leftDirectoryBefore = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = DateTime.Parse("01.01.1990")}
            };
            var leftDirectoryAfter = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = DateTime.Parse("01.01.1991") }
            };

            var leftChange = new Change(ChangeType.Modified, leftDirectoryBefore.GetFile("file1"), leftDirectoryAfter.GetFile("file1"));

            // deletion in right directory
            var rightDirectoryBefore = new Directory("root")
            {
                root => new EmptyFile(root, "file1") {LastWriteTime = DateTime.Parse("01.01.1990")}
            };
            var rightDirectoryAfter = new Directory("root");
            var rightChange = new Change(ChangeType.Deleted, rightDirectoryBefore.GetFile("file1"), null);

            var syncResult = m_Instance.Synchronize(
                GetMockedFileSystemDiff(leftDirectoryBefore, leftDirectoryAfter, leftChange).Object,
                GetMockedFileSystemDiff(rightDirectoryBefore, rightDirectoryAfter, rightChange).Object);
                
            Assert.Empty(syncResult.Actions);
            Assert.Single(syncResult.Conflicts);
            Assert.IsType<ModificationDeletionSyncConflict>(syncResult.Conflicts.Single());
            var action = (ModificationDeletionSyncConflict) syncResult.Conflicts.Single();


            Assert.Equal("file1", action.DeletedFile.Name);
            Assert.Equal(DateTime.Parse("01.01.1990"), action.DeletedFile.LastWriteTime);

            Assert.Equal("file1", action.ModifiedFile.Name);
            Assert.Equal(DateTime.Parse("01.01.1991"), action.ModifiedFile.LastWriteTime);
        }

        [Fact(DisplayName = nameof(Synchronizer) + ".Synchronize(): Modification and Deletion of modified file is handled by deleting the modified file")]
        public void Synchronize_Modification_and_Deletion_of_modified_file_is_handled_by_deleting_the_modified_file()
        {
            // scenario:
            // a file exists in both directories (2 differnet version)
            // in the left directory, the file is replaced by the version in the right directory
            // while the version in the right directory is deleted
            // expected outcome: delete the file in the left directory as well
            
            // modification in left directory
            var leftDirectoryBefore = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = DateTime.Parse("01.01.1990")}
            };
            var leftDirectoryAfter = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = DateTime.Parse("01.01.1991") }
            };

            var leftChange = new Change(ChangeType.Modified, leftDirectoryBefore.GetFile("file1"), leftDirectoryAfter.GetFile("file1"));

            // deletion in right directory
            var rightDirectoryBefore = new Directory("root")
            {
                root => new EmptyFile(root, "file1") {LastWriteTime = DateTime.Parse("01.01.1991")}
            };
            var rightDirectoryAfter = new Directory("root");
            var rightChange = new Change(ChangeType.Deleted, rightDirectoryBefore.GetFile("file1"), null);

            var syncResult = m_Instance.Synchronize(
                GetMockedFileSystemDiff(leftDirectoryBefore, leftDirectoryAfter, leftChange).Object,
                GetMockedFileSystemDiff(rightDirectoryBefore, rightDirectoryAfter, rightChange).Object);
                
            Assert.Empty(syncResult.Conflicts);
            Assert.Single(syncResult.Actions);
            Assert.IsType<RemoveFileSyncAction>(syncResult.Actions.Single());

            var action = (RemoveFileSyncAction) syncResult.Actions.Single();
            Assert.Equal(SyncParticipant.Left, action.Target);
            Assert.Equal("file1", action.RemovedFile.Name);
            Assert.Equal(DateTime.Parse("01.01.1991"), action.RemovedFile.LastWriteTime);

        }
     
        [Fact(DisplayName = nameof(Synchronizer) + ".Synchronize(): Modification and Deletion throws InvalidOperationException if neither modified or unmodified file was deleted")]
        public void Synchronize_Modification_and_Deletion_throws_InvalidOperationException_if_neither_modified_or_unmodified_file_was_deleted()
        {
            var leftDirectoryBefore = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = DateTime.Parse("01.01.1990")}
            };
            var leftDirectoryAfter = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = DateTime.Parse("01.01.1991") }
            };

            var leftChange = new Change(ChangeType.Modified, leftDirectoryBefore.GetFile("file1"), leftDirectoryAfter.GetFile("file1"));

            // deletion in right directory
            var rightDirectoryBefore = new Directory("root")
            {
                root => new EmptyFile(root, "file1") {LastWriteTime = DateTime.Parse("01.01.1992")}
            };
            var rightDirectoryAfter = new Directory("root");
            var rightChange = new Change(ChangeType.Deleted, rightDirectoryBefore.GetFile("file1"), null);

            Assert.Throws<InvalidOperationException>(() =>
                m_Instance.Synchronize(
                    GetMockedFileSystemDiff(leftDirectoryBefore, leftDirectoryAfter, leftChange).Object,
                    GetMockedFileSystemDiff(rightDirectoryBefore, rightDirectoryAfter, rightChange).Object)
                );
        }


        [Fact(DisplayName = nameof(Synchronizer) + ".Synchronize(): Double Modification: Modifications that result in a consistent state yields empty result")]
        public void Synchronize_Double_Modification_Modifications_that_result_in_a_consistent_state_yields_empty_result()
        {
            var lastWriteTime = DateTime.Now;

            var leftDirectoryBefore = new Directory("root")
            {
                root => new EmptyFile(root, "file1") {LastWriteTime = DateTime.Parse("01.01.1989")}
            };
            var leftDirectoryAfter = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = lastWriteTime }
            };
            var leftChange = new Change(ChangeType.Modified, leftDirectoryBefore.GetFile("file1"), leftDirectoryAfter.GetFile("file1"));

            var rightDirectoryBefore = new Directory("root")
            {
                root => new EmptyFile(root, "file1") {LastWriteTime = DateTime.Parse("01.01.1990")}
            };
            var rightDirectoryAfter = new Directory("root")
            {
                root => new EmptyFile(root, "file1") {LastWriteTime = lastWriteTime}
            };
            var rightChange = new Change(ChangeType.Modified, rightDirectoryBefore.GetFile("file1"), rightDirectoryAfter.GetFile("file1"));

            var syncResult = m_Instance.Synchronize(
                GetMockedFileSystemDiff(leftDirectoryBefore, leftDirectoryAfter, leftChange).Object,
                GetMockedFileSystemDiff(rightDirectoryBefore, rightDirectoryAfter, rightChange).Object);

            Assert.Empty(syncResult.Actions);
            Assert.Empty(syncResult.Conflicts);

        }

        [Fact(DisplayName = nameof(Synchronizer) + ".Synchronize(): Double Modification: Modifications not based on each other yield conflict")]
        public void Synchronize_Double_Modification_Modifications_not_based_on_each_other_yield_conflict()
        {
            var lastWriteTime = DateTime.Now;

            var leftDirectoryBefore = new Directory("root")
            {
                root => new EmptyFile(root, "file1") {LastWriteTime = DateTime.Parse("01.01.1990")}
            };
            var leftDirectoryAfter = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = lastWriteTime }
            };
            var leftChange = new Change(ChangeType.Modified, leftDirectoryBefore.GetFile("file1"), leftDirectoryAfter.GetFile("file1"));

            var rightDirectoryBefore = new Directory("root")
            {
                root => new EmptyFile(root, "file1") {LastWriteTime = DateTime.Parse("01.01.1990")}
            };
            var rightDirectoryAfter = new Directory("root")
            {
                root => new EmptyFile(root, "file1") {LastWriteTime = lastWriteTime.AddHours(2)}
            };
            var rightChange = new Change(ChangeType.Modified, rightDirectoryBefore.GetFile("file1"), rightDirectoryAfter.GetFile("file1"));

            var syncResult = m_Instance.Synchronize(
                GetMockedFileSystemDiff(leftDirectoryBefore, leftDirectoryAfter, leftChange).Object,
                GetMockedFileSystemDiff(rightDirectoryBefore, rightDirectoryAfter, rightChange).Object);
                
            Assert.Empty(syncResult.Actions);
            Assert.Single(syncResult.Conflicts);
            Assert.IsType<MultipleVersionSyncConflict>(syncResult.Conflicts.Single());
            var action = (MultipleVersionSyncConflict) syncResult.Conflicts.Single();
            Assert.True(action.ConflictedFiles.All(f => f.Name == "file1"));
            Assert.True(action.ConflictedFiles.SingleOrDefault(f => f.LastWriteTime == rightDirectoryAfter.GetFile("file1").LastWriteTime) != null);
            Assert.True(action.ConflictedFiles.SingleOrDefault(f => f.LastWriteTime == leftDirectoryAfter.GetFile("file1").LastWriteTime) != null);

        }

        [Fact(DisplayName = nameof(Synchronizer) + ".Synchronize(): Double Modification: Conflicts between modifications based on each other can be resolved (1)")]
        public void Synchronize_Double_Modification_Conflicts_between_modifications_based_on_each_other_can_be_resolved_1()
        {
            var lastWriteTime = DateTime.Now;

            // left directory: file version 1 -> 2
            var leftDirectoryBefore = new Directory("root")
            {
                root => new EmptyFile(root, "file1") {LastWriteTime = lastWriteTime.AddHours(1)} // version 1
            };
            var leftDirectoryAfter = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = lastWriteTime.AddHours(2) } // version 2
            };
            var leftChange = new Change(ChangeType.Modified, leftDirectoryBefore.GetFile("file1"), leftDirectoryAfter.GetFile("file1"));

            // right directory: file version 2 -> 3
            var rightDirectoryBefore = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = lastWriteTime.AddHours(2) } // version 2
            };
            var rightDirectoryAfter = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = lastWriteTime.AddHours(3) } // version 3
            };
            var rightChange = new Change(ChangeType.Modified, rightDirectoryBefore.GetFile("file1"), rightDirectoryAfter.GetFile("file1"));

            var syncResult = m_Instance.Synchronize(
                GetMockedFileSystemDiff(leftDirectoryBefore, leftDirectoryAfter, leftChange).Object,
                GetMockedFileSystemDiff(rightDirectoryBefore, rightDirectoryAfter, rightChange).Object);


            // expected result: replace version 2 in left directory with version 3 from right directory

            Assert.Empty(syncResult.Conflicts);
            Assert.Single(syncResult.Actions);
            Assert.IsType<ReplaceFileSyncAction>(syncResult.Actions.Single());
            var action = (ReplaceFileSyncAction)syncResult.Actions.Single();

            Assert.Equal(SyncParticipant.Left, action.Target);
            Assert.Equal(lastWriteTime.AddHours(2), action.OldVersion.LastWriteTime); // version 2
            Assert.Equal(lastWriteTime.AddHours(3), action.NewVersion.LastWriteTime); // version 3

        }

        [Fact(DisplayName = nameof(Synchronizer) + ".Synchronize(): Double Modification: Conflicts between modifications based on each other can be resolved (2)")]
        public void Synchronize_Double_Modification_Conflicts_between_modifications_based_on_each_other_can_be_resolved_2()
        {
            var lastWriteTime = DateTime.Now;

            // left directory: file version 2 -> 3
            var leftDirectoryBefore = new Directory("root")
            {
                root => new EmptyFile(root, "file1") {LastWriteTime = lastWriteTime.AddHours(2)} // version 2
            };
            var leftDirectoryAfter = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = lastWriteTime.AddHours(3) } // version 3
            };
            var leftChange = new Change(ChangeType.Modified, leftDirectoryBefore.GetFile("file1"), leftDirectoryAfter.GetFile("file1"));

            // right directory: file version 1 -> 2
            var rightDirectoryBefore = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = lastWriteTime.AddHours(1) } // version 1
            };
            var rightDirectoryAfter = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = lastWriteTime.AddHours(2) } // version 2
            };
            var rightChange = new Change(ChangeType.Modified, rightDirectoryBefore.GetFile("file1"), rightDirectoryAfter.GetFile("file1"));

            var syncResult = m_Instance.Synchronize(
                GetMockedFileSystemDiff(leftDirectoryBefore, leftDirectoryAfter, leftChange).Object,
                GetMockedFileSystemDiff(rightDirectoryBefore, rightDirectoryAfter, rightChange).Object);                

            // expected result: replace version 2 in left directory with version 3 from right directory

            Assert.Empty(syncResult.Conflicts);
            Assert.Single(syncResult.Actions);
            Assert.IsType<ReplaceFileSyncAction>(syncResult.Actions.Single());
            var action = (ReplaceFileSyncAction)syncResult.Actions.Single();

            Assert.Equal(SyncParticipant.Right, action.Target);
            Assert.Equal(lastWriteTime.AddHours(2), action.OldVersion.LastWriteTime); // version 2
            Assert.Equal(lastWriteTime.AddHours(3), action.NewVersion.LastWriteTime); // version 3
        }

        [Fact(DisplayName = nameof(Synchronizer) + ".Synchronize(): Double Modification: InvalidOperationException is thrown if there is no common base version")]
        public void Synchronize_Double_Modification_InvalidOperationException_is_thrown_if_there_is_no_common_base_version()
        {
            var lastWriteTime = DateTime.Now;

            // left directory: file version 1 -> 2
            var leftDirectoryBefore = new Directory("root")
            {
                root => new EmptyFile(root, "file1") {LastWriteTime = lastWriteTime.AddHours(1)} // version 1
            };
            var leftDirectoryAfter = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = lastWriteTime.AddHours(2) } // version 2
            };
            var leftChange = new Change(ChangeType.Modified, leftDirectoryBefore.GetFile("file1"), leftDirectoryAfter.GetFile("file1"));

            // right directory: file version 3 -> 4
            var rightDirectoryBefore = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = lastWriteTime.AddHours(3) } // version 3
            };
            var rightDirectoryAfter = new Directory("root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = lastWriteTime.AddHours(4) } // version 4
            };
            var rightChange = new Change(ChangeType.Modified, rightDirectoryBefore.GetFile("file1"), rightDirectoryAfter.GetFile("file1"));

            Assert.Throws<InvalidOperationException>(() => m_Instance.Synchronize(
                GetMockedFileSystemDiff(leftDirectoryBefore, leftDirectoryAfter, leftChange).Object,
                GetMockedFileSystemDiff(rightDirectoryBefore, rightDirectoryAfter, rightChange).Object));                
        }



        Mock<IFileSystemDiff> GetMockedFileSystemDiff(IDirectory fromSnapshot, IDirectory toSnapshot, params IChange[] changes)
        {
            var mock = new Mock<IFileSystemDiff>(MockBehavior.Strict);

            if (fromSnapshot != null)
            {
                mock.Setup(m => m.FromSnapshot).Returns(GetMockedSnapshot(fromSnapshot).Object);
            }
            if (toSnapshot != null)
            {
                mock.Setup(m => m.ToSnapshot).Returns(GetMockedSnapshot(toSnapshot).Object);
            }
            if (changes != null)
            {
                mock.Setup(m => m.Changes).Returns(changes);
            }

            return mock;
        }

        Mock<IFileSystemSnapshot> GetMockedSnapshot(IDirectory state)
        {
            var mock = new Mock<IFileSystemSnapshot>(MockBehavior.Strict);
            mock.Setup(m => m.RootDirectory).Returns(state);
            return mock;
        }
    }
}