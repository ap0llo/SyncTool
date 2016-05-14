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
    public class SyncAction : Change
    {

        public SyncActionState State { get; }

        public string Target { get; }

        public Guid Id { get; }

        public int SyncPointId { get; }


        public SyncAction(ChangeType type, IFileReference fromVersion, IFileReference toVersion,  Guid id, string target, SyncActionState state, int syncPointId)
            : base(type, fromVersion, toVersion)
        {
            if (syncPointId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(syncPointId), "Id must be a positive integer");
            }
            
            this.Target = target;
            this.Id = id;
            this.State = state;
            this.SyncPointId = syncPointId;
        }


        public SyncAction WithState(SyncActionState state)
        {
            return new SyncAction(Type, FromVersion, ToVersion, Id, Target, state, SyncPointId);
        }
    }
}