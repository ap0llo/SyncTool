// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.FileSystem.Git;
using Xunit;

namespace SyncTool.Configuration.Git
{
    public class GitBasedSyncGroupTest : DirectoryBasedTest
    {

        [Fact(DisplayName = "Folders is empty for new directory")]
        public void Folders_is_empty_for_new_directory()
        {            
            RepositoryInitHelper.InitializeRepository(m_TempDirectory.Location, "Irrelevant");

            using (var syncGroup = new GitBasedSyncGroup(m_TempDirectory.Location))
            {
                Assert.Empty(syncGroup.Folders);
            }
        }

        [Fact(DisplayName = "Name returns name specified in RepositoryInfo file")]
        public void Name_returns_name_specified_in_RepositoryInfo_file()
        {
            var name = Guid.NewGuid().ToString();

            RepositoryInitHelper.InitializeRepository(m_TempDirectory.Location, name);

            using (var syncGroup = new GitBasedSyncGroup(m_TempDirectory.Location))
            {
                Assert.Equal(name, syncGroup.Name);
            }
        }

        [Fact(Skip = "Not Implemented", DisplayName = "AddSyncGroup creates a new commit in the underlying repository")]
        public void AddSyncGroup_creates_a_new_commit_in_the_underlying_repository()
        {
            throw new NotImplementedException();
        }

        [Fact(Skip = "Not Implemented", DisplayName = "AddSyncGroup writes the SyncGroup to the underlying repository")]
        public void AddSyncGroup_writes_the_SyncGroup_to_the_underlying_repository()
        {
            throw new NotImplementedException();
        }
    }
}