// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        [Fact]
        public void Snapshots_is_empty_for_empty_repository()
        {
            Assert.Empty(m_Instance.Snapshots);
        }


        [Fact]
        public void LatestFileSystemSnapshot_is_null_for_empty_repository()
        {
            Assert.Null(m_Instance.LatestFileSystemSnapshot);
        }


        [Fact]
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

        [Fact]
        public void CreateSnapshot_creates_a_valid_snapshot()
        {
            var directory = new Directory(s_Dir1)
            {
                new EmptyFile(s_File1)
            };

            var snapshot = m_Instance.CreateSnapshot(directory);

            FileSystemAssert.DirectoryEqual(directory, snapshot.RootDirectory);
        }


        [Fact]
        [Trait("foo", "bar")]
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

        [Fact]
        public void CompareSnapshots_throws_a_SnapshotNotFoundException_is_the_id_is_unknown()
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

        [Fact(Skip= "Not implemented yet")]
        public void CompareSnapshots_detects_modification_of_files()
        {
            var state1 = new Directory(s_Dir1)
            {
                new EmptyFile(s_File1) { LastWriteTime = DateTime.Now.AddDays(-2) }
            };

            var state2 = new Directory(s_Dir1)
            {
                new EmptyFile(s_File1) { LastWriteTime = DateTime.Now.AddDays(-1)}
            };

            var snapshot1 = m_Instance.CreateSnapshot(state1);
            var snapshot2 = m_Instance.CreateSnapshot(state2);

            Assert.NotEqual(snapshot1.Id, snapshot2.Id);

            var diff = m_Instance.CompareSnapshots(snapshot1.Id, snapshot2.Id);

            Assert.Equal(diff.FromSnapshot, snapshot1);
            Assert.Equal(diff.ToSnapshot, snapshot2);

            Assert.Equal(1, diff.Changes.Count());
            Assert.Equal(ChangeType.Modified, diff.Changes.Single().Type);

            //TODO compare files in IChange instance
        }


        public override void Dispose()
        {
            m_Repository.Dispose();            
            base.Dispose();
        }
    }
}