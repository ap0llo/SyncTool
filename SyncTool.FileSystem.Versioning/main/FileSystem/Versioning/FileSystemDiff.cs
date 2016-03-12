// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.FileSystem.Versioning
{
    /// <summary>
    /// Immutable default implementation of <see cref="IFileSystemDiff"/>
    /// </summary>
    public class FileSystemDiff : IFileSystemDiff
    {
        public IFileSystemHistory History { get; }

        public IFileSystemSnapshot FromSnapshot { get; }

        public IFileSystemSnapshot ToSnapshot { get; }

        public IEnumerable<IChange> Changes { get; }


        public FileSystemDiff(IFileSystemHistory history, IFileSystemSnapshot toSnapshot, IEnumerable<IChange> changes)
        {
            if (history == null)
            {
                throw new ArgumentNullException(nameof(history));
            }
            if (toSnapshot == null)
            {
                throw new ArgumentNullException(nameof(toSnapshot));
            }
            if (changes == null)
            {
                throw new ArgumentNullException(nameof(changes));
            }
            History = history;
            ToSnapshot = toSnapshot;
            Changes = changes.ToList();
        }

        public FileSystemDiff(IFileSystemHistory history, IFileSystemSnapshot fromSnapshot, IFileSystemSnapshot toSnapshot, IEnumerable<IChange> changes)
        {
            if (history == null)
            {
                throw new ArgumentNullException(nameof(history));
            }

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

            History = history;
            FromSnapshot = fromSnapshot;
            ToSnapshot = toSnapshot;
            Changes = changes.ToList();
        }
    }
}