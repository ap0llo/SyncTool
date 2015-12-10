// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.FileSystem.TestHelpers;
using Xunit;

namespace SyncTool.FileSystem.Git
{
    public class GitBasedGroupTest : DirectoryBasedTest
    {
        [Fact(DisplayName = nameof(GitBasedGroup) + ".Name returns name specified in RepositoryInfo file")]
        public void Name_returns_name_specified_in_RepositoryInfo_file()
        {
            var name = Guid.NewGuid().ToString();

            RepositoryInitHelper.InitializeRepository(m_TempDirectory.Location, name);

            using (var syncGroup = new GitBasedGroup(m_TempDirectory.Location))
            {
                Assert.Equal(name, syncGroup.Name);
            }
        }
    }
}