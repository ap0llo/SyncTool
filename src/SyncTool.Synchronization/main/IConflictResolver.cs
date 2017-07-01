using SyncTool.Common.Groups;

namespace SyncTool.Synchronization
{
    public interface IConflictResolver
    {
        void ResolveConflicts(IGroup group);
    }
}