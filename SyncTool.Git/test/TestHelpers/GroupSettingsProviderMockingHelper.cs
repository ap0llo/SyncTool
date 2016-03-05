// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using SyncTool.Git.Configuration.Model;
using SyncTool.Git.Configuration.Reader;

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
            catch (Exception e)
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