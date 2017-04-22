using SyncTool.Common;

namespace SyncTool.Synchronization
{
    public interface ISynchronizer
    {
        void Synchronize(IGroup group);
    }
}