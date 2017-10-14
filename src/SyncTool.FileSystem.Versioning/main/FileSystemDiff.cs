using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

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

        public IEnumerable<ChangeList> ChangeLists { get; } 


        public FileSystemDiff(
            [NotNull] IFileSystemHistory history,
            [NotNull] IFileSystemSnapshot toSnapshot,
            [NotNull] IEnumerable<ChangeList> changeLists )
        {
            History = history ?? throw new ArgumentNullException(nameof(history));
            ToSnapshot = toSnapshot ?? throw new ArgumentNullException(nameof(toSnapshot));
            ChangeLists = (changeLists ?? throw new ArgumentNullException(nameof(changeLists))).ToList();
        }

        public FileSystemDiff(
            [NotNull] IFileSystemHistory history,
            [NotNull] IFileSystemSnapshot fromSnapshot,
            [NotNull] IFileSystemSnapshot toSnapshot,
            [NotNull] IEnumerable<ChangeList> changeLists)
        {
            History = history ?? throw new ArgumentNullException(nameof(history));
            FromSnapshot = fromSnapshot ?? throw new ArgumentNullException(nameof(fromSnapshot));
            ToSnapshot = toSnapshot ?? throw new ArgumentNullException(nameof(toSnapshot));            
            ChangeLists = (changeLists ?? throw new ArgumentNullException(nameof(changeLists))).ToList();
        }
    }
}