// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.FileSystem;
using SyncTool.Git.FileSystem.Versioning.MetaFileSystem;
using SyncTool.Synchronization.State;

namespace SyncTool.Git.FileSystem
{
    public class SyncPointStateFile : DataFile<ISyncPoint>
    {

        public const string FileNameSuffix = ".SynchronizationState.json";

        public SyncPointStateFile(IDirectory parent, ISyncPoint state) : base(parent, GetFileName(state.Id), state)
        {
        }

        public override IFile WithParent(IDirectory newParent)
        {
            return new  SyncPointStateFile(newParent, Content);
        }


        /// <summary>
        /// Loads a <see cref="SyncPointStateFile"/> written out into a file 
        /// </summary>
        public static SyncPointStateFile Load(IDirectory parentDirectory, IReadableFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (!file.Name.EndsWith(FileNameSuffix, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException($"File name Name must end with {FileNameSuffix}", nameof(file));
            }

            using (var stream = file.OpenRead())
            {
                return new SyncPointStateFile(parentDirectory, stream.Deserialize<MutableSyncPoint>());
            }
        }

        
        public static string GetFileName(int id) => $"{id}{FileNameSuffix}";
    }
}