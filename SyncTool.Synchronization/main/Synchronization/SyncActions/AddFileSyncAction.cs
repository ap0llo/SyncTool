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
        public override string FilePath => NewFile.Path;

        public override ChangeType Type => ChangeType.Added;

        public override IFileReference FromVersion => null;

        public override IFileReference ToVersion => NewFile;


        public IFileReference NewFile { get; }


        public AddFileSyncAction(Guid id, string target, SyncActionState state, int syncPointId, IFileReference newFile) : base(id, target, state, syncPointId)
        {
            if (newFile == null)
            {
                throw new ArgumentNullException(nameof(newFile));
            }
            this.NewFile = newFile;
        }


        public override SyncAction WithState(SyncActionState state) => new AddFileSyncAction(Id, Target, state, SyncPointId, NewFile);
    }
}