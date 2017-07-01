using System;
using System.Linq;
using Xunit;

namespace SyncTool.FileSystem.Versioning
{

    /// <summary>
    /// Tests for <see cref="ChangeList"/>
    /// </summary>
    public class ChangeListTest
    {
        [Fact]
        public void Construtor_throws_ArgumentNullException_if_changes_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ChangeList(null));
        }

        [Fact]
        public void Construtor_throws_ArgumentNullException_if_changes_is_empty()
        {
            Assert.Throws<ArgumentException>(() => new ChangeList(Enumerable.Empty<IChange>()));
        }

        [Fact]
        public void Construtor_throws_ArgumentNullException_if_changes_have_different_paths()
        {
            var file1 = new FileReference("/path1", DateTime.MinValue, 23);
            var file2 = new FileReference("/path2", DateTime.MinValue, 23);

            var change1 = new Change(ChangeType.Added, null, file1);
            var change2 = new Change(ChangeType.Deleted, file2,  null);

            Assert.Throws<ArgumentException>(() => new ChangeList(new [] { change1, change2 }));
        }
    }
}