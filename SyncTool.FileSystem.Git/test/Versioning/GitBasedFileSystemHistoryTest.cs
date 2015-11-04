using System;
using System.Linq;
using LibGit2Sharp;
using SyncTool.FileSystem.Local;
using Xunit;

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


        public override void Dispose()
        {
            base.Dispose();
            m_Repository.Dispose();            
        }
    }
}