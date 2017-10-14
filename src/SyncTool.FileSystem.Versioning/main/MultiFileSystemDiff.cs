using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace SyncTool.FileSystem.Versioning
{
    public class MultiFileSystemDiff : IMultiFileSystemDiff
    {
        public IMultiFileSystemSnapshot FromSnapshot { get; }

        public IMultiFileSystemSnapshot ToSnapshot { get; }
        
        public IEnumerable<MultiFileSystemChangeList> FileChanges { get; }

        public IEnumerable<HistoryChange> HistoryChanges { get; } 


        public MultiFileSystemDiff(
            [NotNull] IMultiFileSystemSnapshot toSnapshot,
            [NotNull] IEnumerable<MultiFileSystemChangeList> fileChanges,
            [NotNull] IEnumerable<HistoryChange> historyChanges)
        {
            FromSnapshot = null;
            ToSnapshot = toSnapshot ?? throw new ArgumentNullException(nameof(toSnapshot));
            FileChanges = fileChanges?.ToArray() ?? throw new ArgumentNullException(nameof(fileChanges));
            HistoryChanges = historyChanges?.ToArray() ?? throw new ArgumentNullException(nameof(historyChanges));
        }

        public MultiFileSystemDiff(
            [NotNull] IMultiFileSystemSnapshot fromSnapshot,
            [NotNull] IMultiFileSystemSnapshot toSnapshot,
            [NotNull] IEnumerable<MultiFileSystemChangeList> fileChanges,
            [NotNull] IEnumerable<HistoryChange> historyChanges)
        {
            FromSnapshot = fromSnapshot ?? throw new ArgumentNullException(nameof(fromSnapshot));
            ToSnapshot = toSnapshot ?? throw new ArgumentNullException(nameof(toSnapshot));
            FileChanges = fileChanges?.ToArray() ?? throw new ArgumentNullException(nameof(fileChanges));
            HistoryChanges = historyChanges?.ToArray() ?? throw new ArgumentNullException(nameof(historyChanges));
        }
    }
}