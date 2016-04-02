// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using Newtonsoft.Json;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization.SyncActions
{
    public sealed class ReplaceFileSyncAction : SyncAction
    {
        [JsonIgnore]
        public override string FilePath => OldVersion.Path;

        public IFileReference OldVersion { get; }

        public IFileReference NewVersion { get; }


        public ReplaceFileSyncAction(Guid id, string target, IFileReference oldVersion, IFileReference newVersion) : base(id, target)
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
        
    }
}