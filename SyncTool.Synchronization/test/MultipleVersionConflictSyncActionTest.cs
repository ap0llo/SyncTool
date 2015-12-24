// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.FileSystem;
using Xunit;

namespace SyncTool.Synchronization
{
    public class MultipleVersionConflictSyncActionTest
    {
        [Fact]
        public void Constructor_checks_parameter_for_null()
        {
            Assert.Throws<ArgumentNullException>(() => new MultipleVersionConflictSyncAction(null));
        }

        [Fact]
        public void Constructor_throws_ArgumentException_if_list_of_conflicted_files_is_empty()
        {
            Assert.Throws<ArgumentException>(() => new MultipleVersionConflictSyncAction());
            Assert.Throws<ArgumentException>(() => new MultipleVersionConflictSyncAction(new IFile[0]));
        }
    }
}