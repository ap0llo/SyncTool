using System;
using System.Collections;
using System.Collections.Generic;
using SyncTool.FileSystem;
using SyncTool.Synchronization.SyncActions;

namespace SyncTool.Synchronization
{
    public class SyncActionFactory
    {
        readonly IEqualityComparer<IFileReference> m_FileReferenceComparer;

        public SyncActionFactory(IEqualityComparer<IFileReference> fileReferenceComparer)
        {
            m_FileReferenceComparer = fileReferenceComparer ?? throw new ArgumentNullException(nameof(fileReferenceComparer));
        }

        public SyncAction GetSyncAction(string targetName, int syncPointId, IFileReference currentFileVersion, IFileReference newFileVersion)
        {
            if (m_FileReferenceComparer.Equals(currentFileVersion, newFileVersion))
            {
                return null;
            }

            if (currentFileVersion != null)
            {
                if (newFileVersion == null)
                {
                    return SyncAction.CreateRemoveFileSyncAction(targetName, SyncActionState.Queued, syncPointId, currentFileVersion);
                }
                else
                {
                    return SyncAction.CreateReplaceFileSyncAction(targetName, SyncActionState.Queued, syncPointId, currentFileVersion, newFileVersion);
                }
            }
            else
            {
                if (newFileVersion != null)
                {
                    return SyncAction.CreateAddFileSyncAction(targetName, SyncActionState.Queued, syncPointId, newFileVersion);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}