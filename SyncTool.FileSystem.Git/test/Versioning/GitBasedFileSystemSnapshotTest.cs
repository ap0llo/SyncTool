using System;
using System.Diagnostics;
using System.Linq;
using Fluent.IO;
using LibGit2Sharp;
using SyncTool.FileSystem.Local;
using Xunit;

namespace SyncTool.FileSystem.Git.Test
{
    public class GitBasedFileSystemSnapshotTest : IDisposable
    {
        
        readonly LocalItemCreator m_DirectoryCreator = new LocalItemCreator();
        readonly TemporaryLocalDirectory m_TempDirectory;



        public GitBasedFileSystemSnapshotTest()
        {
            m_TempDirectory = m_DirectoryCreator.CreateTemporaryDirectory();
            RepositoryInitHelper.InitializeRepository(m_TempDirectory.Location);
        }

        [Fact]
        public void Create_returns_a_new_snapshot_that_equals_the_original_directory()
        {
            var directory = new Directory("root")
            {
                new EmptyFile("file1") { LastWriteTime = DateTime.Now},
                new Directory("dir1")
                {
                    new EmptyFile("file2") { LastWriteTime = DateTime.Now },
                    new Directory("dir2")
                }
            };

            using (var repository = new Repository(m_TempDirectory.Location))
            {
                var snapshot = GitBasedFileSystemSnapshot.Create(repository, repository.Branches["master"], directory);

                AssertDirectoryEquals(directory, snapshot.RootDirectory);
            }
        }


        void AssertDirectoryEquals(IDirectory expected, IDirectory actual)
        {
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Directories.Count(), actual.Directories.Count());
            Assert.Equal(expected.Files.Count(), actual.Files.Count());

            foreach (var directory in expected.Directories)
            {
                Assert.True(actual.DirectoryExists(directory.Name));
                AssertDirectoryEquals(directory, actual.GetDirectory(directory.Name));
            }

            foreach (var file in expected.Files)
            {
                Assert.True(actual.FileExists(file.Name));
                AssertFileEquals(file, actual.GetFile(file.Name));
            }
        }


        void AssertFileEquals(IFile expected, IFile actual)
        {
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Length, actual.Length);
            Assert.Equal(expected.LastWriteTime, actual.LastWriteTime);
        }

        public void Dispose()
        {
            m_TempDirectory.Dispose();
        }
    }
}