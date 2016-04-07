// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.FileSystem;
using Xunit;

namespace SyncTool.Synchronization.SyncActions
{
    /// <summary>
    /// Tests for <see cref="AddFileSyncAction"/>
    /// </summary>
    public class AddFileSyncActionTest
    {
        [Fact]
        public void Constructor_throws_ArgumentOutOfRangeException_if_SyncPointId_is_negative_or_zero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new AddFileSyncAction(
                            Guid.Empty, // irrelevant
                            "target1",   // irrelevant
                            default(SyncActionState), //irrelevant
                            0,
                            new FileReference("/path1")));


            Assert.Throws<ArgumentOutOfRangeException>(
                () => new AddFileSyncAction(
                            Guid.Empty, // irrelevant
                            "target1",   // irrelevant
                            default(SyncActionState), //irrelevant
                            -1,
                            new FileReference("/path1")));
        }
    }
}