// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization.SyncActions
{
    public sealed class RemoveFileSyncAction : SyncAction
    {
        public override string FilePath => RemovedFile.Path;

        public IFileReference RemovedFile { get; }


        public RemoveFileSyncAction(Guid id, string target, IFileReference removedFile) : base(id, target)
        {
            if (removedFile == null)
            {
                throw new ArgumentNullException(nameof(removedFile));
            }
            this.RemovedFile = removedFile;
        }


        public override void Accept<T>(ISyncActionVisitor<T> visitor, T parameter)
        {
            visitor.Visit(this, parameter);
        }
    }
}