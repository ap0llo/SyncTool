// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.FileSystem.TestHelpers;
using Xunit;

namespace SyncTool.Synchronization
{
    public class ResolvedSyncActionTest
    {

        [Fact]
        public void Constructor_throws_ArgumentException_is_SyncActionType_is_ResolveConflict()
        {
            Assert.Throws<ArgumentException>(() => new ResolvedSyncAction(SyncActionType.ResolveConflict, MockingHelper.GetMockedFile(), MockingHelper.GetMockedFile()));
        }

        [Fact]
        public void Constructor_checks_parameters_for_null_based_on_SyncActionType()
        {
            Assert.ThrowsAny<ArgumentException>(() => new ResolvedSyncAction(SyncActionType.AddFile, null, null));
            Assert.ThrowsAny<ArgumentException>(() => new ResolvedSyncAction(SyncActionType.AddFile, MockingHelper.GetMockedFile(), null));
            Assert.ThrowsAny<ArgumentException>(() => new ResolvedSyncAction(SyncActionType.AddFile, MockingHelper.GetMockedFile(), MockingHelper.GetMockedFile()));
            new ResolvedSyncAction(SyncActionType.AddFile, null, MockingHelper.GetMockedFile());

            Assert.ThrowsAny<ArgumentException>(() => new ResolvedSyncAction(SyncActionType.RemoveFile, null, null));
            Assert.ThrowsAny<ArgumentException>(() => new ResolvedSyncAction(SyncActionType.RemoveFile, null, MockingHelper.GetMockedFile()));
            Assert.ThrowsAny<ArgumentException>(() => new ResolvedSyncAction(SyncActionType.RemoveFile, MockingHelper.GetMockedFile(), MockingHelper.GetMockedFile()));
            new ResolvedSyncAction(SyncActionType.RemoveFile, MockingHelper.GetMockedFile(), null);


            Assert.ThrowsAny<ArgumentException>(() => new ResolvedSyncAction(SyncActionType.ReplaceFile, null, null));
            Assert.ThrowsAny<ArgumentException>(() => new ResolvedSyncAction(SyncActionType.ReplaceFile, null, MockingHelper.GetMockedFile()));
            Assert.ThrowsAny<ArgumentException>(() => new ResolvedSyncAction(SyncActionType.ReplaceFile, MockingHelper.GetMockedFile(), null));
            new ResolvedSyncAction(SyncActionType.ReplaceFile, MockingHelper.GetMockedFile(), MockingHelper.GetMockedFile());
        }

        [Fact]
        public void Constructor_throws_ArgumentException_if_file_paths_differ()
        {
            Assert.ThrowsAny<ArgumentException>(
                () => new ResolvedSyncAction(
                            SyncActionType.ReplaceFile,
                            MockingHelper.GetMockedFile("path1"), 
                            MockingHelper.GetMockedFile("path2")));
        }


    }
}