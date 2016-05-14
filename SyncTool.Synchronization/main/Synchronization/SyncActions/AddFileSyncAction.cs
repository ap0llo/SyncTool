// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Synchronization.SyncActions
{
    public sealed class AddFileSyncAction : SyncAction
    {
        public override string Path => ToVersion.Path;

        public override ChangeType Type => ChangeType.Added;

        public override IFileReference FromVersion => null;

        public override IFileReference ToVersion { get; }
        


        public AddFileSyncAction(Guid id, string target, SyncActionState state, int syncPointId, IFileReference toVersion) : base(id, target, state, syncPointId)
        {
            if (toVersion == null)
            {
                throw new ArgumentNullException(nameof(toVersion));
            }
            this.ToVersion = toVersion;
        }


        public override SyncAction WithState(SyncActionState state) => new AddFileSyncAction(Id, Target, state, SyncPointId, ToVersion);
    }
}