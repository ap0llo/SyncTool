using System.Collections.Generic;
using SyncTool.Common;

namespace SyncTool.Git.Configuration.Reader
{
    public interface IGroupSettingsProvider
    {
        IEnumerable<GroupSettings> GetGroupSettings();

        void SaveGroupSettings(IEnumerable<GroupSettings> settings);
    }
}