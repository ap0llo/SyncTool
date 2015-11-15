// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using LibGit2Sharp;
using SyncTool.FileSystem.Local;
using SyncTool.FileSystem.Versioning;
using Xunit;
using NativeFile = System.IO.File;


namespace SyncTool.FileSystem.Git
{
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
            m_Instance = new GitBasedFileSystemHistory(m_Repository, "master");

        }


        #region Snapshots

        [Fact(DisplayName = "GitBasedFileSystemHistory.Snapshots is empty for empty repository")]
        public void Snapshots_is_empty_for_empty_repository()
        {
            Assert.Empty(m_Instance.Snapshots);
        }

        #endregion

        #region LatestFileSystemSnapshot

        [Fact(DisplayName = "GitBasedFileSystemHistory.LatestFileSystemSnapshot is null for empty repository")]
        public void LatestFileSystemSnapshot_is_null_for_empty_repository()
        {
            Assert.Null(m_Instance.LatestFileSystemSnapshot);
        }

        #endregion

        #region CreateSnapshot

        [Fact(DisplayName = "GitBasedFileSystemHistory.CreateSnapshot() can be executed multiple times")]
        public void CreateSnapshot_can_be_executed_multiple_times()
        {            
            var directory1 = new Directory(s_Dir1)
            {
                new EmptyFile(s_File1)
            };

            var directory2 = new Directory(s_Dir2)
            {
                new EmptyFile(s_File2)
            };

            m_Instance.CreateSnapshot(directory1);
            var snapshot2 = m_Instance.CreateSnapshot(directory2);
            
            Assert.Equal(2, m_Instance.Snapshots.Count());
            Assert.Equal(snapshot2, m_Instance.LatestFileSystemSnapshot);
        }

        [Fact(DisplayName = "GitBasedFileSystemHistory.CreateSnapshot() creates a valid snapshot")]
        public void CreateSnapshot_creates_a_valid_snapshot()
        {
            var directory = new Directory(s_Dir1)
            {
                new EmptyFile(s_File1)
            };

            var snapshot = m_Instance.CreateSnapshot(directory);

            FileSystemAssert.DirectoryEqual(directory, snapshot.RootDirectory);
        }

        [Fact(DisplayName = "GitBasedFileSystemHistory.CreateSnapshot() creates a new snapshot if state was modified")]        
        public void CreateSnapshot_creates_a_new_snapshot_if_state_was_modified()
        {
            var dateTime1 = DateTime.Now;
            var dateTime2 = dateTime1.AddDays(-1);

            var state1 = new Directory(s_Dir1)
            {
                new EmptyFile(s_File1) { LastWriteTime = dateTime1 }
            };

            var state2 = new Directory(s_Dir1)
            {
                new EmptyFile(s_File1) {LastWriteTime = dateTime2 }
            };

            var commitCount = m_Repository.Commits.Count();
                     
            var snapshot1 = m_Instance.CreateSnapshot(state1);
            Assert.Equal(commitCount + 1 , m_Repository.Commits.Count());                
            
            var snapshot2 = m_Instance.CreateSnapshot(state2);
            Assert.Equal(commitCount + 2, m_Repository.Commits.Count());                
            
            Assert.NotEqual(snapshot1.Id, snapshot2.Id);
        }

        [Fact(DisplayName = "GitBasedFileSystemHistory.CreateSnapshot() returns previous snapshot if state is unchanged")]
        public void CreateSnapshot_returns_previous_snapshot_if_state_is_unchanged()
        {            
            var state = new Directory(s_Dir1)
            {
                new EmptyFile(s_File1) { LastWriteTime = DateTime.Now, Length = 1234 }
            };

            var snapshot1 = m_Instance.CreateSnapshot(state);
            var snapshot2 = m_Instance.CreateSnapshot(state);

            Assert.Equal(1, m_Instance.Snapshots.Count());
            Assert.Equal(snapshot1.Id, snapshot2.Id);
        }

        #endregion

        #region CompareSnapshots

        [Fact(DisplayName = "GitBasedFileSystemHistory.CompareSnapshots() throws a SnapshotNotFoundException is the Id is unknown")]
        public void CompareSnapshots_throws_a_SnapshotNotFoundException_is_the_Id_is_unknown()
        {
            Assert.Throws<SnapshotNotFoundException>(() => m_Instance.CompareSnapshots("someId", "someOtherId"));

            var directory1 = new Directory(s_Dir1)
            {
                new EmptyFile(s_File1) 
            };

            var directory2 = new Directory(s_Dir2)
            {
                new EmptyFile(s_File1) 
            };

            var snapshot1 = m_Instance.CreateSnapshot(directory1);
            var snapshot2 = m_Instance.CreateSnapshot(directory2);


            Assert.Throws<SnapshotNotFoundException>(() => m_Instance.CompareSnapshots(snapshot1.Id, "someOtherId"));
            Assert.Throws<SnapshotNotFoundException>(() => m_Instance.CompareSnapshots("someId", snapshot2.Id));

        }

        [Fact(DisplayName = "GitBasedFileSystemHistory.CompareSnapshots() detects modification of files")]
        public void CompareSnapshots_detects_modification_of_files()
        {            
            var state1 = new Directory(s_Dir1)
            {
                new EmptyFile(s_File1) { LastWriteTime = DateTime.Now.AddDays(-2) }
            };

            var state2 = new Directory(s_Dir1)
            {
                new EmptyFile(s_File1) { LastWriteTime = DateTime.Now.AddDays(-1) }
            };

            var snapshot1 = m_Instance.CreateSnapshot(state1);
            var snapshot2 = m_Instance.CreateSnapshot(state2);

            Assert.NotEqual(snapshot1.Id, snapshot2.Id);

            var diff = m_Instance.CompareSnapshots(snapshot1.Id, snapshot2.Id);

            Assert.Equal(diff.FromSnapshot, snapshot1);
            Assert.Equal(diff.ToSnapshot, snapshot2);

            Assert.Equal(1, diff.Changes.Count());
            Assert.Equal(ChangeType.Modified, diff.Changes.Single().Type);
            
            FileSystemAssert.FileEqual(state1.GetFile(s_File1), diff.Changes.Single().FromFile);
            FileSystemAssert.FileEqual(state2.GetFile(s_File1), diff.Changes.Single().ToFile);
        }

        [Fact(DisplayName = "GitBasedFileSystemHistory.CompareSnapshots() detects additions of files")]
        public void CompareSnapshots_detects_additions_of_files()
        {
            var file1 = new EmptyFile(s_File1) {LastWriteTime = DateTime.Now.AddDays(-2)};
            var file2 = new EmptyFile(s_File2) {LastWriteTime = DateTime.Now };            
            var state1 = new Directory(s_Dir1) { file1 };
            var state2 = new Directory(s_Dir1) { file1, file2 };
            
            var snapshot1 = m_Instance.CreateSnapshot(state1);
            var snapshot2 = m_Instance.CreateSnapshot(state2);

            Assert.NotEqual(snapshot1.Id, snapshot2.Id);

            var diff = m_Instance.CompareSnapshots(snapshot1.Id, snapshot2.Id);

            Assert.Equal(diff.FromSnapshot, snapshot1);
            Assert.Equal(diff.ToSnapshot, snapshot2);

            Assert.Equal(1, diff.Changes.Count());
            Assert.Equal(ChangeType.Added, diff.Changes.Single().Type);

            Assert.Null(diff.Changes.Single().FromFile);            
            FileSystemAssert.FileEqual(file2, diff.Changes.Single().ToFile);
        }

        [Fact(DisplayName = "GitBasedFileSystemHistory.CompareSnapshots() detects deletions of files")]
        public void CompareSnapshots_detects_deletions_of_files()
        {
            var file1 = new EmptyFile(s_File1) { LastWriteTime = DateTime.Now.AddDays(-2) };            
            var state1 = new Directory(s_Dir1) { file1 };
            var state2 = new Directory(s_Dir1);

            var snapshot1 = m_Instance.CreateSnapshot(state1);
            var snapshot2 = m_Instance.CreateSnapshot(state2);

            Assert.NotEqual(snapshot1.Id, snapshot2.Id);

            var diff = m_Instance.CompareSnapshots(snapshot1.Id, snapshot2.Id);

            Assert.Equal(diff.FromSnapshot, snapshot1);
            Assert.Equal(diff.ToSnapshot, snapshot2);

            Assert.Equal(1, diff.Changes.Count());
            Assert.Equal(ChangeType.Deleted, diff.Changes.Single().Type);

            Assert.Null(diff.Changes.Single().ToFile);
            FileSystemAssert.FileEqual(file1, diff.Changes.Single().FromFile);
        }

        #endregion


        public override void Dispose()
        {
            m_Repository.Dispose();            
            base.Dispose();
        }
    }
}