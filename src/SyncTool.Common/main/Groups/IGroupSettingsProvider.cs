using System.Collections.Generic;

namespace SyncTool.Common.Groups
{
    public interface IGroupSettingsProvider
    {
        IEnumerable<GroupSettings> GetGroupSettings();

        void SaveGroupSettings(IEnumerable<GroupSettings> settings);
    }
}