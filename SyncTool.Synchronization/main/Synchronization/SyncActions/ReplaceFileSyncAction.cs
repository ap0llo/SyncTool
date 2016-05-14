﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using Newtonsoft.Json;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Synchronization.SyncActions
{
    public sealed class ReplaceFileSyncAction : SyncAction
    {
        public override string Path => FromVersion.Path;

        public override ChangeType Type => ChangeType.Modified;

        public override IFileReference FromVersion { get; }

        public override IFileReference ToVersion { get; }
       


        public ReplaceFileSyncAction(Guid id, string target, SyncActionState state, int syncPointId, IFileReference oldVersion, IFileReference newVersion) : base(id, target, state, syncPointId)
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
                throw new ArgumentException($"The paths of {nameof(oldVersion)} and {nameof(newVersion)} are different");
            }

            this.FromVersion = oldVersion;
            this.ToVersion = newVersion;

        }


        public override SyncAction WithState(SyncActionState state) => new ReplaceFileSyncAction(Id, Target, state, SyncPointId, FromVersion, ToVersion);

    }
}