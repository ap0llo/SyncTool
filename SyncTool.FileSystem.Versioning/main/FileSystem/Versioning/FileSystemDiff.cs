// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.FileSystem.Versioning
{
    public class FileSystemDiff : IFileSystemDiff
    {
        public IFileSystemSnapshot FromSnapshot { get; }

        public IFileSystemSnapshot ToSnapshot { get; }

        public IEnumerable<IChange> Changes { get; }

        public FileSystemDiff(IFileSystemSnapshot toSnapshot, IEnumerable<IChange> changes)
        {
            if (toSnapshot == null)
            {
                throw new ArgumentNullException(nameof(toSnapshot));
            }
            if (changes == null)
            {
                throw new ArgumentNullException(nameof(changes));
            }
            ToSnapshot = toSnapshot;
            Changes = changes.ToList();
        }

        public FileSystemDiff(IFileSystemSnapshot fromSnapshot, IFileSystemSnapshot toSnapshot, IEnumerable<IChange> changes)
        {
            if (fromSnapshot == null)
            {
                throw new ArgumentNullException(nameof(fromSnapshot));
            }
            if (toSnapshot == null)
            {
                throw new ArgumentNullException(nameof(toSnapshot));
            }
            if (changes == null)
            {
                throw new ArgumentNullException(nameof(changes));
            }

            FromSnapshot = fromSnapshot;
            ToSnapshot = toSnapshot;
            Changes = changes.ToList();
        }
    }
}