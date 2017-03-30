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

        public IEnumerable<IChangeList> ChangeLists { get; } 

        public FileSystemDiff(IFileSystemHistory history, IFileSystemSnapshot toSnapshot, IEnumerable<IChangeList> changeLists )
        {
            if (history == null)
            {
                throw new ArgumentNullException(nameof(history));
            }
            if (toSnapshot == null)
            {
                throw new ArgumentNullException(nameof(toSnapshot));
            }
            if (changeLists == null)
            {
                throw new ArgumentNullException(nameof(changeLists));
            }

            changeLists = changeLists.ToList();

            History = history;
            ToSnapshot = toSnapshot;
            ChangeLists = changeLists;
        }

        public FileSystemDiff(IFileSystemHistory history, IFileSystemSnapshot fromSnapshot, IFileSystemSnapshot toSnapshot, IEnumerable<IChangeList> changeLists)
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
            if (changeLists == null)
            {
                throw new ArgumentNullException(nameof(changeLists));
            }
            
            changeLists = changeLists.ToList();
            
            History = history;
            FromSnapshot = fromSnapshot;
            ToSnapshot = toSnapshot;            
            ChangeLists = changeLists;
        }
    }
}