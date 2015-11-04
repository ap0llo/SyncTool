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

        const string s_BranchName1 = "master";
        const string s_BranchName2 = "branch/name";

        public GitBasedFileSystemSnapshotTest()
        {
            RepositoryInitHelper.InitializeRepository(m_TempDirectory.Location);

            using (var repo = new Repository(m_TempDirectory.Location))
            {
                repo.CreateBranch(s_BranchName2, repo.Commits.Single(), SignatureHelper.NewSignature());
            }

        }

        [Theory]
        [InlineData(s_BranchName1)]
        [InlineData(s_BranchName2)]
        public void Create_returns_a_new_snapshot_that_equals_the_original_directory(string branchName)
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
                var snapshot = GitBasedFileSystemSnapshot.Create(repository, branchName, directory);

                FileSystemAssert.DirectoryEqual(directory, snapshot.RootDirectory);
            }
        }

    }
}