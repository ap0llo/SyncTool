// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Reflection;
using Moq;
using SyncTool.FileSystem.TestHelpers;
using Xunit;

namespace SyncTool.FileSystem.Versioning
{
    public class ChangeTest
    {
        [Fact(DisplayName = nameof(Change) + ": Constructor checks validity of file parameters for change type")]
        public void Constructor_checks_validity_of_file_parameters_for_change_type()
        {
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Added, null, null));   
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Added, MockingHelper.GetMockedFile(), MockingHelper.GetMockedFile()));   
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Added, MockingHelper.GetMockedFile(), null));
            new Change(ChangeType.Added, null, MockingHelper.GetMockedFile());

            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Deleted, null, null));
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Deleted, MockingHelper.GetMockedFile(), MockingHelper.GetMockedFile()));
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Deleted, null, MockingHelper.GetMockedFile()));
            new Change(ChangeType.Deleted, MockingHelper.GetMockedFile(), null);

            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Modified, null, null));
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Modified, MockingHelper.GetMockedFile(), null));
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Modified, null, MockingHelper.GetMockedFile()));
            new Change(ChangeType.Modified, MockingHelper.GetMockedFile("path1"), MockingHelper.GetMockedFile("path1"));

        }

        [Fact(DisplayName = nameof(Change) + ": Constructor checks that paths of file parameters match")]
        public void Constructor_checks_that_paths_of_file_parameters_match()
        {
            Assert.Throws<ArgumentException>(() => new Change(ChangeType.Modified, MockingHelper.GetMockedFile("path1"), MockingHelper.GetMockedFile("path2")));
            new Change(ChangeType.Modified, MockingHelper.GetMockedFile("path1"), MockingHelper.GetMockedFile("pATh1"));
        }



       

    }
}