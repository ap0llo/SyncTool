// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Linq;
using LibGit2Sharp;
using SyncTool.FileSystem;
using SyncTool.Git.Common;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.Git.FileSystem.Versioning
{
    /// <summary>
    /// Tests for <see cref="GitBasedFileSystemSnapshot"/>
    /// </summary>
    public class GitBasedFileSystemSnapshotTest : DirectoryBasedTest
    {

        const string s_BranchName1 = "master";
        const string s_BranchName2 = "branch/name";

        public GitBasedFileSystemSnapshotTest()
        {
            RepositoryInitHelper.InitializeRepository(m_TempDirectory.Location);

            using (var repo = new Repository(m_TempDirectory.Location))
            {
                repo.CreateBranch(s_BranchName2, repo.Commits.Single());
            }

        }

        [Theory(DisplayName = "GitBasedFileSystemSnapshot.Create() returns a new snapshot that equals the original directory")]
        [InlineData(s_BranchName1)]
        [InlineData(s_BranchName2)]
        public void Create_returns_a_new_snapshot_that_equals_the_original_directory(string branchName)
        {
            var directory = new Directory(null, "root")
            {
                root => new EmptyFile(root, "file1") { LastWriteTime = DateTime.Now},
                root => new Directory(root, "dir1")
                {
                    dir1 => new EmptyFile(dir1, "file2") { LastWriteTime = DateTime.Now },
                    dir1 => new Directory(dir1, "dir2")
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