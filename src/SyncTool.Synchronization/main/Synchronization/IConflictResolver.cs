using SyncTool.Common;

namespace SyncTool.Synchronization
{
    public interface IConflictResolver
    {
        void ResolveConflicts(IGroup group);
    }
}