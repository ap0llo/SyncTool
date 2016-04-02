// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using SyncTool.FileSystem;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.Synchronization.SyncActions
{
    /// <summary>
    /// Tests for <see cref="ReplaceFileSyncAction"/>
    /// </summary>
    public class ReplaceFileSyncActionTest
    {


        [Fact(DisplayName = nameof(ReplaceFileSyncAction) + "Constructor throws ArgumentException if file paths differ")]
        public void Constructor_throws_ArgumentException_if_file_paths_differ()
        {
            Assert.Throws<ArgumentException>(
                () => new ReplaceFileSyncAction(
                            Guid.Empty, // irrelevant
                            "target1",   // irrelevant
                            new FileReference("path1"),
                            new FileReference("path2")));
        }


    }
}