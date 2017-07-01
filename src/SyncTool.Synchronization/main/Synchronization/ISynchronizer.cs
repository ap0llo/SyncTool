using SyncTool.Common.Groups;

namespace SyncTool.Synchronization
{
    public interface ISynchronizer
    {
        void Synchronize(IGroup group);
    }
}