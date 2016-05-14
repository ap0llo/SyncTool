// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;
using Xunit;

namespace SyncTool.Synchronization.SyncActions
{
    /// <summary>
    /// Tests for <see cref="SyncAction"/>
    /// </summary>
    public class SyncActionTest
    {
        [Fact]
        public void Constructor_throws_ArgumentOutOfRangeException_if_SyncPointId_is_negative_or_zero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new SyncAction(
                            ChangeType.Added,  
                            null,
                            new FileReference("/path1"),
                            Guid.Empty, // irrelevant
                            "target1",   // irrelevant
                            default(SyncActionState), //irrelevant
                            0));


            Assert.Throws<ArgumentOutOfRangeException>(
                () => new SyncAction(
                            ChangeType.Added,
                            null,
                            new FileReference("/path1"),
                            Guid.Empty, // irrelevant
                            "target1",   // irrelevant
                            default(SyncActionState), //irrelevant
                            -1));
        }
    }
}