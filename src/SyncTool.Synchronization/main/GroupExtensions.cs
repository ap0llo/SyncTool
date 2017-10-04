using SyncTool.Common.Groups;

namespace SyncTool.Synchronization
{
    public static class GroupExtensions
    {
        public static ISynchronizer GetSynchronizer(this IGroup group) =>
            group.GetService<ISynchronizer>();
    }
}
