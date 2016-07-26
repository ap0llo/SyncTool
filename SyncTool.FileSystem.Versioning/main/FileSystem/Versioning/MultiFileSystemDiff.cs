// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.FileSystem.Versioning
{
    public class MultiFileSystemDiff : IMultiFileSystemDiff
    {
        public IMultiFileSystemSnapshot FromSnapshot { get; }

        public IMultiFileSystemSnapshot ToSnapshot { get; }

        public IEnumerable<IChangeList> ChangeLists { get; }


        public MultiFileSystemDiff(IMultiFileSystemSnapshot toSnapshot, IEnumerable<IChangeList> changeLists)
        {
            if (toSnapshot == null)
                throw new ArgumentNullException(nameof(toSnapshot));

            if (changeLists == null)
                throw new ArgumentNullException(nameof(changeLists));

            FromSnapshot = null;
            ToSnapshot = toSnapshot;
            ChangeLists = changeLists.ToArray();
        }

        public MultiFileSystemDiff(IMultiFileSystemSnapshot fromSnapshot, IMultiFileSystemSnapshot toSnapshot)
        {
            if (fromSnapshot == null)
                throw new ArgumentNullException(nameof(fromSnapshot));

            if (toSnapshot == null)
                throw new ArgumentNullException(nameof(toSnapshot));

            FromSnapshot = fromSnapshot;
            ToSnapshot = toSnapshot;
        }

    }
}