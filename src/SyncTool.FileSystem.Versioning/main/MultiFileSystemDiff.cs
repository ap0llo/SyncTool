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

        public IEnumerable<HistoryChange> HistoryChanges { get; } 


        public MultiFileSystemDiff(IMultiFileSystemSnapshot toSnapshot, IEnumerable<IMultiFileSystemChangeList> fileChanges, IEnumerable<HistoryChange> historyChanges)
        {
            FromSnapshot = null;
            ToSnapshot = toSnapshot ?? throw new ArgumentNullException(nameof(toSnapshot));
            FileChanges = fileChanges?.ToArray() ?? throw new ArgumentNullException(nameof(fileChanges));
            HistoryChanges = historyChanges?.ToArray() ?? throw new ArgumentNullException(nameof(historyChanges));
        }

        public MultiFileSystemDiff(IMultiFileSystemSnapshot fromSnapshot, IMultiFileSystemSnapshot toSnapshot, IEnumerable<IMultiFileSystemChangeList> fileChanges, IEnumerable<HistoryChange> historyChanges)
        {
            FromSnapshot = fromSnapshot ?? throw new ArgumentNullException(nameof(fromSnapshot));
            ToSnapshot = toSnapshot ?? throw new ArgumentNullException(nameof(toSnapshot));
            FileChanges = fileChanges?.ToArray() ?? throw new ArgumentNullException(nameof(fileChanges));
            HistoryChanges = historyChanges?.ToArray() ?? throw new ArgumentNullException(nameof(historyChanges));
        }
    }
}