// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Reflection;
using Moq;
using Xunit;

namespace SyncTool.FileSystem.Versioning
{
    public class ChangeTest
    {
        [Fact(DisplayName = nameof(Change) + ": Constructor checks validity of file parameters for change type")]
        public void Constructor_checks_validity_of_file_parameters_for_change_type()
        {
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Added, null, null));   
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Added, GetFileMock(), GetFileMock()));   
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Added, GetFileMock(), null));
            new Change(ChangeType.Added, null, GetFileMock());

            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Deleted, null, null));
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Deleted, GetFileMock(), GetFileMock()));
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Deleted, null, GetFileMock()));
            new Change(ChangeType.Deleted, GetFileMock(), null);

            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Modified, null, null));
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Modified, GetFileMock(), null));
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Modified, null, GetFileMock()));
            new Change(ChangeType.Modified, GetFileMock(), GetFileMock());

        }

        [Fact(DisplayName = nameof(Change) + ": Constructor checks that paths of file parameters match")]
        public void Constructor_checks_that_paths_of_file_parameters_match()
        {
            Assert.Throws<ArgumentException>(() => new Change(ChangeType.Modified, GetFileMock("path1"), GetFileMock("path2")));
            new Change(ChangeType.Modified, GetFileMock("path1"), GetFileMock("pATh1"));
        }



        IFile GetFileMock()
        {
            return new Mock<IFile>().Object;
        }

        IFile GetFileMock(string path)
        {
            var mock = new Mock<IFile>(MockBehavior.Strict);
            mock.Setup(m => m.Path).Returns(path);
            return mock.Object;
        }

    }
}