using SyncTool.Common;
using SyncTool.Configuration.Model;

namespace SyncTool.Configuration
{
    public static class GroupExtensions
    {
        public static IConfigurationService GetConfigurationService(this IGroup group) => group.GetService<IConfigurationService>();
    }
}