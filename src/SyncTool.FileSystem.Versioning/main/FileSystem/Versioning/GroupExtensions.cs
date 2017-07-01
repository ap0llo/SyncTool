using SyncTool.Common.Groups;

namespace SyncTool.FileSystem.Versioning
{
    public static class GroupExtensions
    {

        public static IHistoryService GetHistoryService(this IGroup group) => group.GetService<IHistoryService>();

       
    }
}
