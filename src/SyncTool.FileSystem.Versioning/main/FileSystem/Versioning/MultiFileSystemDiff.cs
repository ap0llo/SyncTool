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

        
        public IEnumerable<IMultiFileSystemChangeList> FileChanges { get; }

        public IEnumerable<IHistoryChange> HistoryChanges { get; } 


        public MultiFileSystemDiff(IMultiFileSystemSnapshot toSnapshot, IEnumerable<IMultiFileSystemChangeList> fileChanges, IEnumerable<IHistoryChange> historyChanges)
        {
            if (toSnapshot == null)
                throw new ArgumentNullException(nameof(toSnapshot));

            if (fileChanges == null)
                throw new ArgumentNullException(nameof(fileChanges));

            if (historyChanges == null)
                throw new ArgumentNullException(nameof(historyChanges));

            FromSnapshot = null;
            ToSnapshot = toSnapshot;
            FileChanges = fileChanges.ToArray();
            HistoryChanges = historyChanges.ToArray();
        }

        public MultiFileSystemDiff(IMultiFileSystemSnapshot fromSnapshot, IMultiFileSystemSnapshot toSnapshot, IEnumerable<IMultiFileSystemChangeList> fileChanges, IEnumerable<IHistoryChange> historyChanges)
        {
            if (fromSnapshot == null)
                throw new ArgumentNullException(nameof(fromSnapshot));

            if (toSnapshot == null)
                throw new ArgumentNullException(nameof(toSnapshot));

            if (fileChanges == null)
                throw new ArgumentNullException(nameof(fileChanges));

            if (historyChanges == null)
                throw new ArgumentNullException(nameof(historyChanges));

            FromSnapshot = fromSnapshot;
            ToSnapshot = toSnapshot;
            FileChanges = fileChanges.ToArray();
            HistoryChanges = historyChanges.ToArray();
        }

    }
}