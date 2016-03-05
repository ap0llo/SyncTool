// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using SyncTool.Git.Common;
using SyncTool.Git.FileSystem;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.Git.Common
{
    /// <summary>
    /// Tests for <see cref="GitBasedGroup"/>
    /// </summary>
    public class GitBasedGroupTest : DirectoryBasedTest
    {
        [Fact(DisplayName = nameof(GitBasedGroup) + ".Name must not be null or empty")]
        public void Name_must_not_be_null_or_empty()
        {
            Assert.Throws<ArgumentNullException>(() => new GitBasedGroup(null, m_TempDirectory.Location));

            Assert.Throws<ArgumentException>(() => new GitBasedGroup("", m_TempDirectory.Location));
            Assert.Throws<ArgumentException>(() => new GitBasedGroup("  ", m_TempDirectory.Location));
            Assert.Throws<ArgumentException>(() => new GitBasedGroup(" \t ", m_TempDirectory.Location));
        }


    }
}