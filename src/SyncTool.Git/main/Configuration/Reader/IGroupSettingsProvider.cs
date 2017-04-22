using System.Collections.Generic;
using SyncTool.Git.Configuration.Model;

namespace SyncTool.Git.Configuration.Reader
{
    public interface IGroupSettingsProvider
    {
        IEnumerable<GroupSettings> GetGroupSettings();

        void SaveGroupSettings(IEnumerable<GroupSettings> settings);
    }
}