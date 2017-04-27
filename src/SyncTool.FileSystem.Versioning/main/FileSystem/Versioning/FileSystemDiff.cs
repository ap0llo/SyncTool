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
            History = history ?? throw new ArgumentNullException(nameof(history));
            ToSnapshot = toSnapshot ?? throw new ArgumentNullException(nameof(toSnapshot));
            ChangeLists = (changeLists ?? throw new ArgumentNullException(nameof(changeLists))).ToList();
        }

        public FileSystemDiff(IFileSystemHistory history, IFileSystemSnapshot fromSnapshot, IFileSystemSnapshot toSnapshot, IEnumerable<IChangeList> changeLists)
        {
            History = history ?? throw new ArgumentNullException(nameof(history));
            FromSnapshot = fromSnapshot ?? throw new ArgumentNullException(nameof(fromSnapshot));
            ToSnapshot = toSnapshot ?? throw new ArgumentNullException(nameof(toSnapshot));            
            ChangeLists = (changeLists ?? throw new ArgumentNullException(nameof(changeLists))).ToList();
        }
    }
}