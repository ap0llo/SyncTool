// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Data.Common;
using System.Security.AccessControl;
using Moq;
using SyncTool.FileSystem.TestHelpers;
using SyncTool.FileSystem.Versioning;
using Xunit;

namespace SyncTool.Synchronization
{
    public class GroupedChangeTest
    {
        [Fact(DisplayName = nameof(GroupedChange) + ": Constructor throws ArgumentNullException if both local and global change is null")]
        public void Constructor_throws_ArgumentNullException_if_both_local_and_global_change_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupedChange(null, null));

        }

        [Fact(DisplayName = nameof(GroupedChange) + ": Constructor throws ArgumentException if paths of local and global change do not match")]
        public void Constructor_throws_ArgumentException_if_paths_of_local_and_global_change_do_not_match()
        {
            Assert.Throws<ArgumentException>(() => new GroupedChange(GetChangeMock("path1"), GetChangeMock("path2")));

            // this should work without exception
            new GroupedChange(GetChangeMock("path1"), GetChangeMock("paTH1"));
        }

        public IChange GetChangeMock(string path)
        {
            var mock = new Mock<IChange>();
            mock.Setup(m => m.Path).Returns(path);
            return mock.Object;
        }
    }
}