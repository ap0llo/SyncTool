using System;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using SyncTool.FileSystem;
using SyncTool.FileSystem.TestHelpers;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.FileSystem.Versioning;
using SyncTool.Git.RepositoryAccess;
using Xunit;
using Directory = SyncTool.FileSystem.Directory;
using NativeFile = System.IO.File;


namespace SyncTool.Git.Test.FileSystem.Versioning
{
    /// <summary>
    /// Tests for <see cref="GitBasedFileSystemHistory"/>
    /// </summary>
    public class GitBasedFileSystemHistoryTest : DirectoryBasedTest
    {
        const string s_Dir1 = "dir1";
        const string s_Dir2 = "dir2";
        const string s_File1 = "file1";
        const string s_File2 = "file2";

        
        readonly Repository m_Repository;
        readonly GitBasedFileSystemHistory m_Instance;


        public GitBasedFileSystemHistoryTest()
        { 
            RepositoryInitHelper.InitializeRepository(m_TempDirectory.Location);

            m_Repository = new Repository(m_TempDirectory.Location);
            var branchName = new BranchName("", "branch1");
            m_Repository.CreateBranch(branchName, m_Repository.GetAllCommits().Single());

            m_Instance = new GitBasedFileSystemHistory(m_Repository, branchName);

        }


        #region Snapshots

        [Fact]
        public void Snapshots_is_empty_for_empty_repository()
        {
            Assert.Empty(m_Instance.Snapshots);
        }

        #endregion

        #region LatestFileSystemSnapshot

        [Fact]
        public void LatestFileSystemSnapshot_is_null_for_empty_repository()
        {
            Assert.Null(m_Instance.LatestFileSystemSnapshot);
        }

        #endregion

        #region CreateSnapshot

        [Fact]
        public void CreateSnapshot_can_be_executed_multiple_times()
        {            
            var directory1 = new Directory(s_Dir1)
            {
                d => new EmptyFile(d, s_File1)
            };

            var directory2 = new Directory(s_Dir2)
            {
                d => new EmptyFile(d, s_File2)
            };

            m_Instance.CreateSnapshot(directory1);
            var snapshot2 = m_Instance.CreateSnapshot(directory2);
            
            Assert.Equal(2, m_Instance.Snapshots.Count());
            Assert.Equal(snapshot2, m_Instance.LatestFileSystemSnapshot);
        }

        [Fact]
        public void CreateSnapshot_creates_a_valid_snapshot()
        {
            var directory = new Directory(s_Dir1)
            {
                d => new EmptyFile(d, s_File1)
            };

            var snapshot = m_Instance.CreateSnapshot(directory);

            FileSystemAssert.DirectoryEqual(directory, snapshot.RootDirectory);
        }

        [Fact]
        public void CreateSnapshot_creates_a_new_snapshot_if_state_was_modified()
        {
            var dateTime1 = DateTime.Now;
            var dateTime2 = dateTime1.AddDays(-1);

            var state1 = new Directory(s_Dir1)
            {
                dir1 => new EmptyFile(dir1, s_File1) { LastWriteTime = dateTime1 }
            };

            var state2 = new Directory(s_Dir1)
            {
                dir1 => new EmptyFile(dir1, s_File1) {LastWriteTime = dateTime2 }
            };

            var commitCount = m_Repository.GetAllCommits().Count();
                     
            var snapshot1 = m_Instance.CreateSnapshot(state1);
            Assert.Equal(commitCount + 1 , m_Repository.GetAllCommits().Count());                
            
            var snapshot2 = m_Instance.CreateSnapshot(state2);
            Assert.Equal(commitCount + 2, m_Repository.GetAllCommits().Count());                
            
            Assert.NotEqual(snapshot1.Id, snapshot2.Id);
        }

        [Fact]
        public void CreateSnapshot_returns_previous_snapshot_if_state_is_unchanged()
        {            
            var state = new Directory(s_Dir1)
            {
                d => new EmptyFile(d, s_File1) { LastWriteTime = DateTime.Now, Length = 1234 }
            };

            var snapshot1 = m_Instance.CreateSnapshot(state);
            var snapshot2 = m_Instance.CreateSnapshot(state);

            Assert.Equal(1, m_Instance.Snapshots.Count());
            Assert.Equal(snapshot1.Id, snapshot2.Id);
        }

        #endregion

        #region GetChanges

        [Fact]
        public void GetChanges_throws_a_SnapshotNotFoundException_is_the_Id_is_unknown()
        {
            Assert.Throws<SnapshotNotFoundException>(() => m_Instance.GetChanges("someId", "someOtherId"));

            var directory1 = new Directory(s_Dir1)
            {
                d => new EmptyFile(d, s_File1) 
            };

            var directory2 = new Directory(s_Dir2)
            {
                d => new EmptyFile(d, s_File1) 
            };

            var snapshot1 = m_Instance.CreateSnapshot(directory1);
            var snapshot2 = m_Instance.CreateSnapshot(directory2);


            Assert.Throws<SnapshotNotFoundException>(() => m_Instance.GetChanges(snapshot1.Id, "someOtherId"));
            Assert.Throws<SnapshotNotFoundException>(() => m_Instance.GetChanges("someId", snapshot2.Id));
            Assert.Throws<SnapshotNotFoundException>(() => m_Instance.GetChanges("someId"));

            Assert.Throws<SnapshotNotFoundException>(() => m_Instance.GetChanges(snapshot1.Id, "someOtherId", Array.Empty<string>()));
            Assert.Throws<SnapshotNotFoundException>(() => m_Instance.GetChanges("someId", snapshot2.Id, Array.Empty<string>()));
            Assert.Throws<SnapshotNotFoundException>(() => m_Instance.GetChanges("someId", Array.Empty<string>()));

        }

        [Fact]
        public void GetChanges_detects_modification_of_files()
        {

            //ARRANGE

            var state1 = new Directory(s_Dir1)
            {
                d => new EmptyFile(d, s_File1) { LastWriteTime = DateTime.Now.AddDays(-2) }
            };

            var state2 = new Directory(s_Dir1)
            {
                d => new EmptyFile(d, s_File1) { LastWriteTime = DateTime.Now.AddDays(-1) }
            };

            var snapshot1 = m_Instance.CreateSnapshot(state1);
            var snapshot2 = m_Instance.CreateSnapshot(state2);

            Assert.NotEqual(snapshot1.Id, snapshot2.Id);

            //ACT

            var diff = m_Instance.GetChanges(snapshot1.Id, snapshot2.Id);


            //ASSERT

            Assert.Equal(diff.FromSnapshot, snapshot1);
            Assert.Equal(diff.ToSnapshot, snapshot2);        

            Assert.Single(diff.ChangeLists);
            Assert.Single(diff.ChangeLists.Single().Changes);
            Assert.Equal(ChangeType.Modified, diff.ChangeLists.Single().Changes.Single().Type);

            FileSystemAssert.FileReferenceMatches(state1.GetFile(s_File1), diff.ChangeLists.Single().Changes.Single().FromVersion);
            FileSystemAssert.FileReferenceMatches(state2.GetFile(s_File1), diff.ChangeLists.Single().Changes.Single().ToVersion);
        }

        [Fact]
        public void GetChanges_detects_additions_of_files()
        {
            //ARRANGE

            var writeTime1 = DateTime.Now.AddDays(-2);

            var state1 = new Directory(s_Dir1)
            {
                d => new EmptyFile(d, s_File1) { LastWriteTime = writeTime1 }
            };
            var state2 = new Directory(s_Dir1)
            {
                d => new EmptyFile(d, s_File1) {LastWriteTime = writeTime1 },
                d => new EmptyFile(d, s_File2) {LastWriteTime = DateTime.Now }
            };
            
            var snapshot1 = m_Instance.CreateSnapshot(state1);            
            var snapshot2 = m_Instance.CreateSnapshot(state2);

            Assert.NotEqual(snapshot1.Id, snapshot2.Id);

            //ACT

            var diff = m_Instance.GetChanges(snapshot1.Id, snapshot2.Id);


            //ASSERT

            Assert.Equal(diff.FromSnapshot, snapshot1);
            Assert.Equal(diff.ToSnapshot, snapshot2);
            
            Assert.Single(diff.ChangeLists);

            var changes = diff.ChangeLists.Single().Changes.ToList();
            Assert.Single(changes);
            Assert.Equal(ChangeType.Added, changes.Single().Type);
            FileSystemAssert.FileReferenceMatches(state2.GetFile(s_File2), changes.Single().ToVersion);
        }

        [Fact]
        public void GetChanges_detects_deletions_of_files()
        {
            //ARRANGE

            var state1 = new Directory(s_Dir1)
            {
                d => new EmptyFile(d, s_File1) { LastWriteTime = DateTime.Now.AddDays(-2) }
            };
            var state2 = new Directory(s_Dir1);

            var snapshot1 = m_Instance.CreateSnapshot(state1);
            var snapshot2 = m_Instance.CreateSnapshot(state2);

            Assert.NotEqual(snapshot1.Id, snapshot2.Id);

            //ACT

            var diff = m_Instance.GetChanges(snapshot1.Id, snapshot2.Id);

            //ASSERT

            Assert.Equal(diff.FromSnapshot, snapshot1);
            Assert.Equal(diff.ToSnapshot, snapshot2);
            
            Assert.Single(diff.ChangeLists);
            var changes = diff.ChangeLists.Single().Changes.ToList();
            Assert.Single(changes);
            Assert.Equal(ChangeType.Deleted, changes.Single().Type);

            Assert.Null(changes.Single().ToVersion);
            FileSystemAssert.FileReferenceMatches(state1.GetFile(s_File1), changes.Single().FromVersion);
        }

        [Fact]
        public void GetChanges_ignores_Additions_of_empty_directories()
        {
            //ARRANGE

            var state1 = new Directory(s_Dir1);
            var state2 = new Directory(s_Dir1)
            {
                dir1 => new Directory(dir1, s_Dir2)
            };       

            var snapshot1 = m_Instance.CreateSnapshot(state1);
            var snapshot2 = m_Instance.CreateSnapshot(state2);

            Assert.NotEqual(snapshot1.Id, snapshot2.Id);

            //ACT
            var diff = m_Instance.GetChanges(snapshot1.Id, snapshot2.Id);

            //ASSERT
            Assert.Empty(diff.ChangeLists);       
        }

        [Fact]
        public void GetChanges_ignores_Deletions_of_empty_directories()
        {            
            //ARRANGE

            var state1 = new Directory(s_Dir1)
            {
                dir1 => new Directory(dir1, s_Dir2)
            };

            var state2 = new Directory(s_Dir1);

            var snapshot1 = m_Instance.CreateSnapshot(state1);
            var snapshot2 = m_Instance.CreateSnapshot(state2);

            Assert.NotEqual(snapshot1.Id, snapshot2.Id);

            //ACT
            var diff = m_Instance.GetChanges(snapshot1.Id, snapshot2.Id);

            //ASSERT
            Assert.Empty(diff.ChangeLists);
        }

        [Fact]
        public void GetChanges_ignores_Changes_outside_of_the_Snapshot_directory()
        {
            //ARRANGE

            var state1 = new Directory(s_Dir1);
            var state2 = new Directory(s_Dir1)
            {
                dir1 => new EmptyFile(dir1, "file1")
            };

            var snapshot1 = m_Instance.CreateSnapshot(state1);

            // create unrelated change between two snapshots
            using (var workingDirectory = new TemporaryWorkingDirectory(m_Repository.Info.Path, m_Instance.Id))
            {
                NativeFile.WriteAllText(Path.Combine(workingDirectory.Location, "foo"), "Hello World");
                workingDirectory.Commit();
                workingDirectory.Push();
            }

            var snapshot2 = m_Instance.CreateSnapshot(state2);

            Assert.NotEqual(snapshot1.Id, snapshot2.Id);

            //ACT

            var diff = m_Instance.GetChanges(snapshot1.Id, snapshot2.Id);

            //ASSERT
            Assert.Single(diff.ChangeLists);
        }

        [Fact]
        public void GetChanges_with_single_id_gets_all_changes_since_the_initial_commit()
        {
            //ARRANGE

            var state2 = new Directory(s_Dir1)
            {
                dir1 => new EmptyFile(dir1, "file1")
            };

            var snapshot = m_Instance.CreateSnapshot(state2);

            //ACT

            var diff = m_Instance.GetChanges(snapshot.Id);

            //ASSERT

            Assert.NotNull(diff);
            Assert.Null(diff.FromSnapshot);
            Assert.NotNull(diff.ToSnapshot);
            
            // "new" Changes API
            Assert.Single(diff.ChangeLists);
            var changes = diff.ChangeLists.Single().Changes.ToList();
            Assert.Equal(ChangeType.Added, changes.Single().Type);           
        }

        [Fact]
        public void GetChanges_Multiple_changes_to_the_same_file()
        {
            //ARRANGE

            var lastWriteTime = DateTime.Now;
            var state1 = new Directory(s_Dir1)
            { 
                dir1 => new EmptyFile(dir1, "file1") { LastWriteTime = lastWriteTime.AddHours(1)}
            };
            var state2 = new Directory(s_Dir1)
            {
                dir1 => new EmptyFile(dir1, "file1") { LastWriteTime = lastWriteTime.AddHours(2)}
            };

            var snapshot1 = m_Instance.CreateSnapshot(state1);
            var snapshot2 = m_Instance.CreateSnapshot(state2);

            //ACT

            var diff = m_Instance.GetChanges(snapshot2.Id);

            //ASSERT

            Assert.NotNull(diff);
            Assert.Null(diff.FromSnapshot);
            Assert.NotNull(diff.ToSnapshot);
            
            Assert.Single(diff.ChangeLists);
            var changes = diff.ChangeLists.Single().Changes.ToArray();
            // both changes should be contained in a single ChangeList
            Assert.Equal(2, changes.Length);

            // addition
            Assert.Equal(ChangeType.Added, changes[0].Type);
            Assert.Null(changes[0].FromVersion);
            FileSystemAssert.FileReferenceMatches(state1.GetFile("file1"), changes[0].ToVersion);

            // modification
            Assert.Equal(ChangeType.Modified, changes[1].Type);
            FileSystemAssert.FileReferenceMatches(state1.GetFile("file1"), changes[1].FromVersion);
            FileSystemAssert.FileReferenceMatches(state2.GetFile("file1"), changes[1].ToVersion);
        }

        [Fact]
        public void GetChanges_A_file_gets_added_modified_and_deleted()
        {
            //ARRANGE

            var lastWriteTime = DateTime.Now;
            var state1 = new Directory(s_Dir1)
            {
                dir1 => new EmptyFile(dir1, "file1") { LastWriteTime = lastWriteTime.AddHours(1)}
            };
            var state2 = new Directory(s_Dir1)
            {
                dir1 => new EmptyFile(dir1, "file1") { LastWriteTime = lastWriteTime.AddHours(2)}
            };
            var state3 = new Directory(s_Dir1); // file1 deleted

            var snapshot1 = m_Instance.CreateSnapshot(state1);
            var snapshot2 = m_Instance.CreateSnapshot(state2);
            var snapshot3 = m_Instance.CreateSnapshot(state3);

            //ACT

            var diff = m_Instance.GetChanges(snapshot3.Id);

            //ASSERT


            Assert.NotNull(diff);
            Assert.Null(diff.FromSnapshot);
            Assert.NotNull(diff.ToSnapshot);
            
            // GetChanges() gets all the changes to the file 

            Assert.Single(diff.ChangeLists);
            var changes = diff.ChangeLists.Single().Changes.ToArray();
            // both changes should be contained in a single ChangeList
            Assert.Equal(3, changes.Length);

            // addition
            Assert.Equal(ChangeType.Added, changes[0].Type);
            Assert.Null(changes[0].FromVersion);
            FileSystemAssert.FileReferenceMatches(state1.GetFile("file1"), changes[0].ToVersion);

            // modification
            Assert.Equal(ChangeType.Modified, changes[1].Type);
            FileSystemAssert.FileReferenceMatches(state1.GetFile("file1"), changes[1].FromVersion);
            FileSystemAssert.FileReferenceMatches(state2.GetFile("file1"), changes[1].ToVersion);

            // deletion
            Assert.Equal(ChangeType.Deleted, changes[2].Type);
            FileSystemAssert.FileReferenceMatches(state2.GetFile("file1"), changes[2].FromVersion);
            Assert.Null(changes[2].ToVersion);

        }

        [Fact]
        public void GetChanges_A_file_gets_added_modified_and_deleted_between_snapshots()
        {
            //ARRANGE

            var lastWriteTime = DateTime.Now;
            var state0 = new Directory(s_Dir1);
            var state1 = new Directory(s_Dir1)
            {
                dir1 => new EmptyFile(dir1, "file1") { LastWriteTime = lastWriteTime.AddHours(1)}
            };
            var state2 = new Directory(s_Dir1)
            {
                dir1 => new EmptyFile(dir1, "file1") { LastWriteTime = lastWriteTime.AddHours(2)}
            };
            var state3 = new Directory(s_Dir1); // file1 deleted

            var snapshot0 = m_Instance.CreateSnapshot(state0);            
            var snapshot1 = m_Instance.CreateSnapshot(state1);
            var snapshot2 = m_Instance.CreateSnapshot(state2);
            var snapshot3 = m_Instance.CreateSnapshot(state3);

            //ACT

            var diff = m_Instance.GetChanges(snapshot0.Id, snapshot3.Id);


            //ASSERT

            Assert.NotNull(diff);
            Assert.Equal(snapshot0, diff.FromSnapshot);
            Assert.Equal(snapshot3, diff.ToSnapshot);
            
            Assert.Single(diff.ChangeLists);
            var changes = diff.ChangeLists.Single().Changes.ToArray();
            // both changes should be contained in a single ChangeList
            Assert.Equal(3, changes.Length);

            // addition
            Assert.Equal(ChangeType.Added, changes[0].Type);
            Assert.Null(changes[0].FromVersion);
            FileSystemAssert.FileReferenceMatches(state1.GetFile("file1"), changes[0].ToVersion);

            // modification
            Assert.Equal(ChangeType.Modified, changes[1].Type);
            FileSystemAssert.FileReferenceMatches(state1.GetFile("file1"), changes[1].FromVersion);
            FileSystemAssert.FileReferenceMatches(state2.GetFile("file1"), changes[1].ToVersion);

            // deletion
            Assert.Equal(ChangeType.Deleted, changes[2].Type);
            FileSystemAssert.FileReferenceMatches(state2.GetFile("file1"), changes[2].FromVersion);
            Assert.Null(changes[2].ToVersion);

        }

        [Fact]
        public void GetChages_returns_empty_result_if_empty_path_filter_is_supplied()
        {
            //ARRANGE            
            var state = new Directory(s_Dir1) { dir1 => new EmptyFile(dir1, "file1") { LastWriteTime = DateTime.Now} };            
            var snapshot = m_Instance.CreateSnapshot(state);
            
            //ACT
            var diff = m_Instance.GetChanges(snapshot.Id, Array.Empty<string>());

            //ASSERT            
            Assert.Empty(diff.ChangeLists);            
        }

        [Fact]
        public void GetChanges_returns_filtered_list_of_changes_if_path_filter_is_supplied()
        {
            //ARRANGE

            var state = new Directory(s_Dir1)
            {
                dir1 => new EmptyFile(dir1, "file1"),   // Add file1
                dir1 => new EmptyFile(dir1, "file2"),   // Add file2
                dir1 => new Directory(dir1, s_Dir2)
                {
                    dir2 => new EmptyFile(dir2, "file3")
                }
            };            

            var snapshot = m_Instance.CreateSnapshot(state);
            
            //ACT + ASSERT

            // path if only applied to "new" Changes API            
            Assert.Single(m_Instance.GetChanges(snapshot.Id, new[] { "/file1" }).ChangeLists);
            Assert.Single(m_Instance.GetChanges(snapshot.Id, new[] { "/file2" }).ChangeLists);
            Assert.Single(m_Instance.GetChanges(snapshot.Id, new[] { "/dir2/file3" }).ChangeLists);
            Assert.Equal(2, m_Instance.GetChanges(snapshot.Id, new[] { "/dir2/file3", "/file1" }).ChangeLists.Count());
            Assert.Equal(3, m_Instance.GetChanges(snapshot.Id, null).ChangeLists.Count());
        }

        
        [Fact]
        public void GetChanges_throws_FormatException_if_path_filter_contains_relative_paths()
        {
            // all paths passed to GetChanges() must be rooted
            Assert.Throws<FormatException>(() => m_Instance.GetChanges("irrelevant", new string[] { "fileName" }));
            Assert.Throws<FormatException>(() => m_Instance.GetChanges("irrelevant", new string[] { "relative/path/to/file" }));
        }

        [Fact]
        public void GetChanges_throws_FormatException_if_path_filter_contains_invalid_paths()
        {
            Assert.Throws<ArgumentNullException>(() => m_Instance.GetChanges("irrelevant", new string[] { null }));
            Assert.Throws<FormatException>(() => m_Instance.GetChanges("irrelevant", new string[] { "" }));
            Assert.Throws<FormatException>(() => m_Instance.GetChanges("irrelevant", new string[] { " " }));
            Assert.Throws<FormatException>(() => m_Instance.GetChanges("irrelevant", new string[] { "\t" }));
            Assert.Throws<FormatException>(() => m_Instance.GetChanges("irrelevant", new string[] { "/" }));
            
        }


        [Fact]
        public void GetChanges_ChangeList_Paths_are_rooted()
        {
            var state = new Directory(s_Dir1)
            {
                dir1 => new EmptyFile(dir1, "file1")                
            };

            var snapshot = m_Instance.CreateSnapshot(state);
            
            Assert.Single(m_Instance.GetChanges(snapshot.Id).ChangeLists);
            Assert.True(m_Instance.GetChanges(snapshot.Id).ChangeLists.Single().Path.StartsWith("/"));
        }

        [Fact]
        public void GetChanges_throws_InvalidRangeException()
        {
            var state1 = new Directory(s_Dir1) { dir1 => new EmptyFile(dir1, "file1") };
            var snapshot1 = m_Instance.CreateSnapshot(state1);

            var state2 = new Directory(s_Dir1) { dir1 => new EmptyFile(dir1, "file2") };
            var snapshot2 = m_Instance.CreateSnapshot(state2);

            Assert.Throws<InvalidRangeException>(() => m_Instance.GetChanges(snapshot2.Id, snapshot1.Id));
        }


        #endregion

        #region GetChangedFiles

        [Fact]
        public void GetChangedFiles_throws_a_SnapshotNotFoundException_is_the_Id_is_unknown()
        {
            Assert.Throws<SnapshotNotFoundException>(() => m_Instance.GetChangedFiles("someId", "someOtherId"));

            var directory1 = new Directory(s_Dir1)
            {
                d => new EmptyFile(d, s_File1)
            };

            var directory2 = new Directory(s_Dir2)
            {
                d => new EmptyFile(d, s_File1)
            };

            var snapshot1 = m_Instance.CreateSnapshot(directory1);
            var snapshot2 = m_Instance.CreateSnapshot(directory2);


            Assert.Throws<SnapshotNotFoundException>(() => m_Instance.GetChangedFiles(snapshot1.Id, "someOtherId"));
            Assert.Throws<SnapshotNotFoundException>(() => m_Instance.GetChangedFiles("someId", snapshot2.Id));
            Assert.Throws<SnapshotNotFoundException>(() => m_Instance.GetChangedFiles("someId"));            
        }

        [Fact]
        public void GetChangedFiles_detects_modification_of_files()
        {
            //ARRANGE
            var state1 = new Directory(s_Dir1)
            {
                d => new EmptyFile(d, s_File1) { LastWriteTime = DateTime.Now.AddDays(-2) }
            };
            var state2 = new Directory(s_Dir1)
            {
                d => new EmptyFile(d, s_File1) { LastWriteTime = DateTime.Now.AddDays(-1) }
            };

            var snapshot1 = m_Instance.CreateSnapshot(state1);
            var snapshot2 = m_Instance.CreateSnapshot(state2);

            Assert.NotEqual(snapshot1.Id, snapshot2.Id);

            //ACT
            var changedFiles = m_Instance.GetChangedFiles(snapshot1.Id, snapshot2.Id);

            //ASSERT            
            Assert.Single(changedFiles);
            Assert.Equal(state2.GetFile(s_File1).Path, changedFiles.Single());
        }

        [Fact]
        public void GetChangedFiles_detects_additions_of_files()
        {
            //ARRANGE
            var writeTime1 = DateTime.Now.AddDays(-2);
            var state1 = new Directory(s_Dir1)
            {
                d => new EmptyFile(d, s_File1) { LastWriteTime = writeTime1 }
            };
            var state2 = new Directory(s_Dir1)
            {
                d => new EmptyFile(d, s_File1) {LastWriteTime = writeTime1 },
                d => new EmptyFile(d, s_File2) {LastWriteTime = DateTime.Now }
            };

            var snapshot1 = m_Instance.CreateSnapshot(state1);
            var snapshot2 = m_Instance.CreateSnapshot(state2);

            Assert.NotEqual(snapshot1.Id, snapshot2.Id);

            //ACT
            var changedFiles = m_Instance.GetChangedFiles(snapshot1.Id, snapshot2.Id);
            
            //ASSERT        
            
            Assert.Single(changedFiles);
            Assert.Equal(state2.GetFile(s_File2).Path, changedFiles.Single());
        }

        [Fact]
        public void GetChangedFiles_detects_deletions_of_files()
        {
            //ARRANGE
            var state1 = new Directory(s_Dir1)
            {
                d => new EmptyFile(d, s_File1) { LastWriteTime = DateTime.Now.AddDays(-2) }
            };
            var state2 = new Directory(s_Dir1);

            var snapshot1 = m_Instance.CreateSnapshot(state1);
            var snapshot2 = m_Instance.CreateSnapshot(state2);
            
            //ACT
            var changedFiles = m_Instance.GetChangedFiles(snapshot1.Id, snapshot2.Id);

            //ASSERT
            
            Assert.Single(changedFiles);            
            Assert.Equal(state1.GetFile(s_File1).Path, changedFiles.Single());
        }

        [Fact]
        public void GetChangedFiles_ignores_Additions_of_empty_directories()
        {
            //ARRANGE
            var state1 = new Directory(s_Dir1);
            var state2 = new Directory(s_Dir1)
            {
                dir1 => new Directory(dir1, s_Dir2)
            };

            var snapshot1 = m_Instance.CreateSnapshot(state1);
            var snapshot2 = m_Instance.CreateSnapshot(state2);
            
            //ACT
            var changedFiles = m_Instance.GetChangedFiles(snapshot1.Id, snapshot2.Id);

            //ASSERT
            Assert.Empty(changedFiles);
        }

        [Fact]
        public void GetChangedFiles_ignores_Deletions_of_empty_directories()
        {
            //ARRANGE
            var state1 = new Directory(s_Dir1)
            {
                dir1 => new Directory(dir1, s_Dir2)
            };

            var state2 = new Directory(s_Dir1);

            var snapshot1 = m_Instance.CreateSnapshot(state1);
            var snapshot2 = m_Instance.CreateSnapshot(state2);
            
            //ACT
            var changedFiles = m_Instance.GetChangedFiles(snapshot1.Id, snapshot2.Id);

            //ASSERT
            Assert.Empty(changedFiles);
        }

        [Fact]
        public void GetChangedFiles_ignores_Changes_outside_of_the_Snapshot_directory()
        {
            //ARRANGE
            var state1 = new Directory(s_Dir1);
            var state2 = new Directory(s_Dir1)
            {
                dir1 => new EmptyFile(dir1, "file1")
            };

            var snapshot1 = m_Instance.CreateSnapshot(state1);

            // create unrelated change between two snapshots
            using (var workingDirectory = new TemporaryWorkingDirectory(m_Repository.Info.Path, m_Instance.Id))
            {
                NativeFile.WriteAllText(Path.Combine(workingDirectory.Location, "foo"), "Hello World");
                workingDirectory.Commit();
                workingDirectory.Push();
            }

            var snapshot2 = m_Instance.CreateSnapshot(state2);

            Assert.NotEqual(snapshot1.Id, snapshot2.Id);

            //ACT
            var changedFiles = m_Instance.GetChangedFiles(snapshot1.Id, snapshot2.Id);

            //ASSERT
            Assert.Single(changedFiles);
        }

        [Fact]
        public void GetChangedFiles_with_single_id_gets_all_changes_since_the_initial_commit()
        {
            //ARRANGE

            var state2 = new Directory(s_Dir1)
            {
                dir1 => new EmptyFile(dir1, "file1")
            };

            var snapshot = m_Instance.CreateSnapshot(state2);

            //ACT
            var changedFiles = m_Instance.GetChangedFiles(snapshot.Id);

            //ASSERT
            Assert.Single(changedFiles);            
        }

        [Fact]
        public void GetChangedFiles_Multiple_changes_to_the_same_file()
        {
            //ARRANGE
            var lastWriteTime = DateTime.Now;
            var state1 = new Directory(s_Dir1)
            {
                dir1 => new EmptyFile(dir1, "file1") { LastWriteTime = lastWriteTime.AddHours(1)}
            };
            var state2 = new Directory(s_Dir1)
            {
                dir1 => new EmptyFile(dir1, "file1") { LastWriteTime = lastWriteTime.AddHours(2)}
            };

            var snapshot1 = m_Instance.CreateSnapshot(state1);
            var snapshot2 = m_Instance.CreateSnapshot(state2);

            //ACT
            var changedFiles = m_Instance.GetChangedFiles(snapshot2.Id);

            //ASSERT                    
            Assert.Single(changedFiles);                        
        }

        [Fact]
        public void GetChangedFiles_A_file_gets_added_modified_and_deleted()
        {
            //ARRANGE
            var lastWriteTime = DateTime.Now;
            var state1 = new Directory(s_Dir1)
            {
                dir1 => new EmptyFile(dir1, "file1") { LastWriteTime = lastWriteTime.AddHours(1)}
            };
            var state2 = new Directory(s_Dir1)
            {
                dir1 => new EmptyFile(dir1, "file1") { LastWriteTime = lastWriteTime.AddHours(2)}
            };
            var state3 = new Directory(s_Dir1); // file1 deleted

            var snapshot1 = m_Instance.CreateSnapshot(state1);
            var snapshot2 = m_Instance.CreateSnapshot(state2);
            var snapshot3 = m_Instance.CreateSnapshot(state3);

            //ACT        

            var changedFiles = m_Instance.GetChangedFiles(snapshot3.Id);

            //ASSERT            
            Assert.Single(changedFiles);

            // addition            
        }

        [Fact]
        public void GetChangedFiles_A_file_gets_added_modified_and_deleted_between_snapshots()
        {
            //ARRANGE

            var lastWriteTime = DateTime.Now;
            var state0 = new Directory(s_Dir1);
            var state1 = new Directory(s_Dir1)
            {
                dir1 => new EmptyFile(dir1, "file1") { LastWriteTime = lastWriteTime.AddHours(1)}
            };
            var state2 = new Directory(s_Dir1)
            {
                dir1 => new EmptyFile(dir1, "file1") { LastWriteTime = lastWriteTime.AddHours(2)}
            };
            var state3 = new Directory(s_Dir1); // file1 deleted

            var snapshot0 = m_Instance.CreateSnapshot(state0);
            var snapshot1 = m_Instance.CreateSnapshot(state1);
            var snapshot2 = m_Instance.CreateSnapshot(state2);
            var snapshot3 = m_Instance.CreateSnapshot(state3);

            //ACT

            var changedFiles = m_Instance.GetChangedFiles(snapshot0.Id, snapshot3.Id);


            //ASSERT
            Assert.Single(changedFiles);            
        }

        [Fact]
        public void GetChangedFiles_ChangeList_Paths_are_rooted()
        {
            var state = new Directory(s_Dir1)
            {
                dir1 => new EmptyFile(dir1, "file1")
            };

            var snapshot = m_Instance.CreateSnapshot(state);

            var changedFiles = m_Instance.GetChangedFiles(snapshot.Id);

            Assert.Single(changedFiles);
            Assert.True(changedFiles.Single().StartsWith("/"));
        }


        [Fact]
        public void GetChangedFiles_throws_InvalidRangeException()
        {
            var state1 = new Directory(s_Dir1) { dir1 => new EmptyFile(dir1, "file1") };
            var snapshot1 = m_Instance.CreateSnapshot(state1);

            var state2 = new Directory(s_Dir1) { dir1 => new EmptyFile(dir1, "file2") };
            var snapshot2 = m_Instance.CreateSnapshot(state2);

            Assert.Throws<InvalidRangeException>(() => m_Instance.GetChangedFiles(snapshot2.Id, snapshot1.Id));
        }

        #endregion

        #region Indexer

        [Fact]
        public void Indexer_throws_SnapshotNotFoundException_for_unknown_snapshot_id()
        {
            Assert.Throws<SnapshotNotFoundException>(() => m_Instance["SomeId"]);
        }

        [Fact]
        public void Indexer_throws_ArgumentNullException_if_snapshot_id_is_null_or_empty()
        {
            Assert.Throws<ArgumentNullException>(() => m_Instance[null]);
            Assert.Throws<ArgumentNullException>(() => m_Instance[""]);
            Assert.Throws<ArgumentNullException>(() => m_Instance[" "]);
            Assert.Throws<ArgumentNullException>(() => m_Instance["\t"]);
        }


        [Fact]
        public void Indexer_returns_expected_snapshot()
        {
            var directory1 = new Directory(s_Dir1)
            {
                d => new EmptyFile(d, s_File1)
            };

            var directory2 = new Directory(s_Dir2)
            {
                d => new EmptyFile(d, s_File2)
            };

            var snapshot1 = m_Instance.CreateSnapshot(directory1);
            var snapshot2 = m_Instance.CreateSnapshot(directory2);

            Assert.Equal(snapshot1, m_Instance[snapshot1.Id]);
            Assert.Equal(snapshot2, m_Instance[snapshot2.Id]);

            // snapshot ids are case-invariant
            Assert.Equal(snapshot1, m_Instance[snapshot1.Id.ToLower()]);
            Assert.Equal(snapshot2, m_Instance[snapshot2.Id.ToLower()]);

            Assert.Equal(snapshot1, m_Instance[snapshot1.Id.ToUpper()]);
            Assert.Equal(snapshot2, m_Instance[snapshot2.Id.ToUpper()]);
        }

        #endregion

        #region GetPreviousSnapshotId

        [Fact]
        public void GetPreviousSnapshotId_throws_a_SnapshotNotFoundException_is_the_Id_is_unknown()
        {
            Assert.Throws<SnapshotNotFoundException>(() => m_Instance.GetPreviousSnapshotId("someId"));            
        }
        
        [Fact]
        public void GetPreviousSnapshotId_returns_null_if_a_snapshot_is_the_first_snapshot()
        {
            var state = new Directory(s_Dir1)
            {
                dir1 => new EmptyFile(dir1, "file1")
            };

            var snapshot = m_Instance.CreateSnapshot(state);

            Assert.Null(m_Instance.GetPreviousSnapshotId(snapshot.Id));
        }


        [Fact]
        public void GetPreviousSnapshotId_returns_the_expected_value()
        {
            var state1 = new Directory(s_Dir1)
            {
                dir1 => new EmptyFile(dir1, "file1")
            };

            var state2 = new Directory(s_Dir1)
            {
                dir1 => new EmptyFile(dir1, "file1"),
                dir1 => new EmptyFile(dir1, "file2")
            };

            var snapshot1 = m_Instance.CreateSnapshot(state1);
            var snapshot2 = m_Instance.CreateSnapshot(state2);

            Assert.NotNull(m_Instance.GetPreviousSnapshotId(snapshot2.Id));
            Assert.Equal(snapshot1.Id, m_Instance.GetPreviousSnapshotId(snapshot2.Id));
        }

        #endregion

        public override void Dispose()
        {
            m_Repository.Dispose();            
            base.Dispose();
        }
    }
}