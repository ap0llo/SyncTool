// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Moq;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;
using Xunit;

namespace SyncTool.Synchronization
{
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

            Assert.Empty(result);
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
                GetMockedFileSystemDiff(rightDirectory, rightDirectory, Array.Empty<IChange>()).Object)
                .ToList();

            Assert.Single(result);
            Assert.IsType<AddFileSyncAction>(result.Single());

            var syncAction = (AddFileSyncAction) result.Single();
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
                GetMockedFileSystemDiff(rightDirectory, rightDirectory, Array.Empty<IChange>()).Object)
                .ToList();

            Assert.Single(result);
            Assert.IsType<ReplaceFileSyncAction>(result.Single());

            var syncAction = (ReplaceFileSyncAction) result.Single();
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
                GetMockedFileSystemDiff(rightDirectory, rightDirectory, Array.Empty<IChange>()).Object)
                .ToList();

            Assert.Single(result);
            Assert.IsType<RemoveFileSyncAction>(result.Single());

            var syncAction = (RemoveFileSyncAction) result.Single();
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
                GetMockedFileSystemDiff(rightDirectory, rightDirectory, Array.Empty<IChange>()).Object)
                .ToList();
            Assert.Empty(result);
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


            var syncActions = m_Instance.Synchronize(
                GetMockedFileSystemDiff(leftDirectoryBefore, leftDirectoryAfter, leftChanges).Object,
                GetMockedFileSystemDiff(rightDirectoryBefore, rightDirectoryAfter, rightChanges).Object)
                .Cast<AddFileSyncAction>()
                .ToList();

            Assert.Equal(2, syncActions.Count());

            var leftSyncAction = syncActions.Single(action => action.Target == SyncParticipant.Left);
            var rightSyncAction = syncActions.Single(action => action.Target == SyncParticipant.Right);

            Assert.Equal("dir2/file2", leftSyncAction.NewFile.Path);
            Assert.Equal("dir1/file1", rightSyncAction.NewFile.Path);
        }


        Mock<IFileSystemDiff> GetMockedFileSystemDiff(IDirectory fromSnapshot, IDirectory toSnapshot, IChange[] changes)
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