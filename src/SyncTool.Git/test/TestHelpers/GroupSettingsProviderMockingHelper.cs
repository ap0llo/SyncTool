using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using SyncTool.Common;

namespace SyncTool.Git.TestHelpers
{
    public static class GroupSettingsProviderMockingHelper
    {
        public static Mock<IGroupSettingsProvider> GetGroupSettingsProviderMock()
        {
            var mock = new Mock<IGroupSettingsProvider>(MockBehavior.Strict);
            mock.Setup(m => m.SaveGroupSettings(It.IsAny<IEnumerable<GroupSettings>>()));
            return mock;
        }

        public static Mock<IGroupSettingsProvider> WithEmptyGroupSettings(this Mock<IGroupSettingsProvider> mock)
        {
            mock.Setup(m => m.GetGroupSettings()).Returns(Enumerable.Empty<GroupSettings>());
            return mock;
        }


        public static Mock<IGroupSettingsProvider> WithGroup(this Mock<IGroupSettingsProvider> mock, string name, string address)
        {
            IEnumerable<GroupSettings> current;
            try
            {
                current = mock.Object.GetGroupSettings();
            }
            catch (Exception)
            {
                current = Enumerable.Empty<GroupSettings>();
            }

            var group = new GroupSettings()
            {
                Name = name,
                Address = address
            };

            mock.Setup(m => m.GetGroupSettings()).Returns(current.Union(new[] { group }).ToList());

            return mock;
        }
    }
}