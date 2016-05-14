// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using Newtonsoft.Json;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Synchronization.SyncActions
{
    public abstract class SyncAction : IChange
    {
        public abstract string Path { get; }

        public ChangeType Type { get; }

        public IFileReference FromVersion { get; }
        
        public IFileReference ToVersion { get; }


        public SyncActionState State { get; }

        public string Target { get; }

        public Guid Id { get; }

        public int SyncPointId { get; }


        protected SyncAction(ChangeType type, IFileReference fromVersion, IFileReference toVersion,  Guid id, string target, SyncActionState state, int syncPointId)
        {
            if (syncPointId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(syncPointId), "Id must be a positive integer");
            }

            this.Type = type;
            this.FromVersion = fromVersion;
            this.ToVersion = toVersion;

            this.Target = target;
            this.Id = id;
            this.State = state;
            this.SyncPointId = syncPointId;
        }


        public abstract SyncAction WithState(SyncActionState state);

    }
}