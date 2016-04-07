// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015-2016, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.FileSystem.Versioning
{
    /// <summary>
    /// Tests for <see cref="Change"/>
    /// </summary>
    public class ChangeTest
    {
        [Fact(DisplayName = nameof(Change) + ": Constructor checks validity of file parameters for change type")]
        public void Constructor_checks_validity_of_file_parameters_for_change_type()
        {
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Added, null, null));   
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Added, FileMockingHelper.GetMockedFile("/path1", DateTime.Now, 42), FileMockingHelper.GetMockedFile("/path1", DateTime.Now, 42)));   
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Added, FileMockingHelper.GetMockedFile("/path1", DateTime.Now, 42), null));
            new Change(ChangeType.Added, null, FileMockingHelper.GetMockedFile("/path1", DateTime.Now, 42));

            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Deleted, null, null));
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Deleted, FileMockingHelper.GetMockedFile("/path1", DateTime.Now, 42), FileMockingHelper.GetMockedFile("/path1", DateTime.Now, 42)));
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Deleted, null, FileMockingHelper.GetMockedFile("/path1", DateTime.Now, 42)));
            new Change(ChangeType.Deleted, FileMockingHelper.GetMockedFile("/path1", DateTime.Now, 42), null);

            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Modified, null, null));
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Modified, FileMockingHelper.GetMockedFile("/path1", DateTime.Now, 42), null));
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Modified, null, FileMockingHelper.GetMockedFile("/path1", DateTime.Now, 42)));
            new Change(ChangeType.Modified, FileMockingHelper.GetMockedFile("/path1", DateTime.Now, 42), FileMockingHelper.GetMockedFile("/path1", DateTime.Now, 42));

        }

        [Fact(DisplayName = nameof(Change) + ": Constructor checks that paths of file parameters match")]
        public void Constructor_checks_that_paths_of_file_parameters_match()
        {
            Assert.Throws<ArgumentException>(() => new Change(ChangeType.Modified, FileMockingHelper.GetMockedFile("/path1", DateTime.Now, 42), FileMockingHelper.GetMockedFile("/path2", DateTime.Now, 42)));
            new Change(ChangeType.Modified, FileMockingHelper.GetMockedFile("/path1", DateTime.Now, 42), FileMockingHelper.GetMockedFile("/pATh1", DateTime.Now, 42));
        }



       

    }
}