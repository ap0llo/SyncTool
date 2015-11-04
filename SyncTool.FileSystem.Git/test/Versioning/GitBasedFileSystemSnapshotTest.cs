using System;
using System.Diagnostics;
using System.Linq;
using Fluent.IO;
using LibGit2Sharp;
using SyncTool.FileSystem.Local;
using Xunit;

namespace SyncTool.FileSystem.Git
{
    public class GitBasedFileSystemSnapshotTest : DirectoryBasedTest
    {
        

        public GitBasedFileSystemSnapshotTest()
        {
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
                var snapshot = GitBasedFileSystemSnapshot.Create(repository, "master", directory);

                FileSystemAssert.DirectoryEqual(directory, snapshot.RootDirectory);
            }
        }

    }
}