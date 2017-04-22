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
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Added, new FileReference("/path1", DateTime.Now, 42), new FileReference("/path1", DateTime.Now, 42)));   
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Added, new FileReference("/path1", DateTime.Now, 42), null));
            new Change(ChangeType.Added, null, new FileReference("/path1", DateTime.Now, 42));

            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Deleted, null, null));
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Deleted, new FileReference("/path1", DateTime.Now, 42), new FileReference("/path1", DateTime.Now, 42)));
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Deleted, null, new FileReference("/path1", DateTime.Now, 42)));
            new Change(ChangeType.Deleted, new FileReference("/path1", DateTime.Now, 42), null);

            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Modified, null, null));
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Modified, new FileReference("/path1", DateTime.Now, 42), null));
            Assert.ThrowsAny<ArgumentException>(() => new Change(ChangeType.Modified, null, new FileReference("/path1", DateTime.Now, 42)));
            new Change(ChangeType.Modified, new FileReference("/path1", DateTime.Now, 42), new FileReference("/path1", DateTime.Now, 42));

        }

        [Fact(DisplayName = nameof(Change) + ": Constructor checks that paths of file parameters match")]
        public void Constructor_checks_that_paths_of_file_parameters_match()
        {
            Assert.Throws<ArgumentException>(() => new Change(ChangeType.Modified, new FileReference("/path1", DateTime.Now, 42), new FileReference("/path2", DateTime.Now, 42)));
            new Change(ChangeType.Modified, new FileReference("/path1", DateTime.Now, 42), new FileReference("/pATh1", DateTime.Now, 42));
        }



       

    }
}