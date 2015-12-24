// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization
{
    public sealed class ReplaceFileSyncAction : ResolvedSyncAction
    {

        public IFile OldVersion { get; }

        public IFile NewVersion { get; }


        public ReplaceFileSyncAction(SyncParticipant target, IFile oldVersion, IFile newVersion) : base(target)
        {
            if (oldVersion == null)
            {
                throw new ArgumentNullException(nameof(oldVersion));
            }
            if (newVersion == null)
            {
                throw new ArgumentNullException(nameof(newVersion));
            }
            if (!StringComparer.InvariantCultureIgnoreCase.Equals(oldVersion.Path, newVersion.Path))
            {
                throw new ArgumentException($"The paths of {nameof(oldVersion)} and {nameof(newVersion)} are differnet");
            }

            this.OldVersion = oldVersion;
            this.NewVersion = newVersion;

        }

        public override void Accept(ISyncActionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}