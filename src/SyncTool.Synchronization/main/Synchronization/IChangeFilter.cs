using System;
using SyncTool.FileSystem.Versioning;
using SyncTool.Synchronization.SyncActions;

namespace SyncTool.Synchronization
{
    public interface IChangeFilter : IEquatable<IChangeFilter>
    {
        bool IncludeInResult(IChangeList changeList);

        bool IncludeInResult(IChange change);

    }
}