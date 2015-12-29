// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using LibGit2Sharp;
using SyncTool.Common;
using SyncTool.Git.Common;
using SyncTool.Git.FileSystem;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.Git.Common
{
    /// <summary>
    /// Tests for <see cref="GitBasedGroupManager{T}"/>
    /// </summary>
    public class GitBasedGroupManagerTest : DirectoryBasedTest
    {
        [Fact(DisplayName = nameof(GitBasedGroupManager<IGroup>) + ": Constructor throws ArgumentNullExcetopn if path provider is null")]
        public void Constructor_throws_ArgumentNullException_if_path_provider_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new TestGroupManager(null));
        }


        [Fact(DisplayName = nameof(GitBasedGroupManager<IGroup>) + ".Groups is empty for empty directory")]
        public void Groups_is_empty_for_empty_directory()
        {
            var groupManager = new TestGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));
            Assert.Empty(groupManager.Groups);
        }


        [Fact(DisplayName = nameof(GitBasedGroupManager<IGroup>) + ".Groups: Non Git repositories are ignored")]
        public void Groups_Non_Git_repositories_are_ignored()
        {
            System.IO.Directory.CreateDirectory(Path.Combine(m_TempDirectory.Location, "dir1"));
            System.IO.Directory.CreateDirectory(Path.Combine(m_TempDirectory.Location, "dir2"));

            var groupManager = new TestGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));
            Assert.Empty(groupManager.Groups);
        }


        [Fact(DisplayName = nameof(GitBasedGroupManager<IGroup>) + ".Groups: Non Bare Repositories are ignored")]
        public void Groups_Non_Bare_Repositories_are_ignored()
        {
            var dirPath = Path.Combine(m_TempDirectory.Location, "dir1");
            System.IO.Directory.CreateDirectory(dirPath);

            Repository.Init((dirPath));

            var groupManager = new TestGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));
            Assert.Empty(groupManager.Groups);
        }

        [Fact(DisplayName = nameof(GitBasedGroupManager<IGroup>) + ".GetRepositoryPath() throws GroupNotFoundException")]
        public void GetRepositoryPath_throws_GroupNotFoundException()
        {
            var groupManager = new TestGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));
            Assert.Throws<GroupNotFoundException>(() => groupManager.GetRepositoryPath("SomeName"));
        }


        [Fact(DisplayName = nameof(GitBasedGroupManager<IGroup>) + ".GetRepositoryPath() returns expected path")]
        public void GetRepositoryPath_returns_expected_path()
        {
            var expectedPath = Path.Combine(m_TempDirectory.Location, "dir1");
            System.IO.Directory.CreateDirectory(expectedPath);

            var name = "SomeName";

            RepositoryInitHelper.InitializeRepository(expectedPath, name);


            var groupManager = new TestGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));
            var actualPath = groupManager.GetRepositoryPath(name);

            Assert.Equal(expectedPath, actualPath);
        }



      

        private class TestGroupManager : GitBasedGroupManager<IGroup>
        {
            public TestGroupManager(IRepositoryPathProvider pathProvider) : base(pathProvider)
            {
            }

            public override IGroup GetGroup(string name)
            {
                throw new NotImplementedException();
            }

            public new string GetRepositoryPath(string name) => base.GetRepositoryPath(name);
        }

    }
}