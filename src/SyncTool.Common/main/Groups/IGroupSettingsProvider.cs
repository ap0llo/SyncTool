using System.Collections.Generic;
using SyncTool.Common;

namespace SyncTool.Common.Groups
{
    public interface IGroupSettingsProvider
    {
        IEnumerable<GroupSettings> GetGroupSettings();

        void SaveGroupSettings(IEnumerable<GroupSettings> settings);
    }
}