using SyncTool.Common.Groups;
using SyncTool.Synchronization.State;

namespace SyncTool.Synchronization
{
    public static class GroupExtensions
    {
        public static ISynchronizer GetSynchronizer(this IGroup group) =>
            group.GetService<ISynchronizer>();

        public static ISyncStateService GetSyncStateService(this IGroup group) =>
            group.GetService<ISyncStateService>();
    }
}
