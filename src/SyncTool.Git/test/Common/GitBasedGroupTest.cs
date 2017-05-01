using System;
using System.Collections.Generic;
using Moq;
using SyncTool.FileSystem;
using SyncTool.Git.Common;
using SyncTool.Git.FileSystem;
using SyncTool.TestHelpers;
using Xunit;
using Autofac;
using SyncTool.Common;

namespace SyncTool.Git.Common
{
    /// <summary>
    /// Tests for <see cref="Group"/>
    /// </summary>
    public class GitBasedGroupTest : DirectoryBasedTest
    {
        [Fact(DisplayName = nameof(Group) + ".Name must not be null or empty")]
        public void Name_must_not_be_null_or_empty()
        {
            var pathProviderMock = new Mock<IRepositoryPathProvider>(MockBehavior.Strict);
            var scopeMock = new Mock<ILifetimeScope>(MockBehavior.Strict);

            Assert.Throws<ArgumentNullException>(() => new Group(new GroupSettings() {Name = null, Address = m_TempDirectory.Location }, scopeMock.Object));

            Assert.Throws<ArgumentException>(() => new Group(new GroupSettings() { Name = "", Address = m_TempDirectory.Location }, scopeMock.Object));
            Assert.Throws<ArgumentException>(() => new Group(new GroupSettings() { Name = "  ", Address = m_TempDirectory.Location }, scopeMock.Object));
            Assert.Throws<ArgumentException>(() => new Group(new GroupSettings() { Name = " \t ", Address = m_TempDirectory.Location }, scopeMock.Object));
        }


    }
}