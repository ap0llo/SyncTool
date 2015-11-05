using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.FileSystem.Git
{
    public class FileSystemDiff : IFileSystemDiff
    {
        public IFileSystemSnapshot FromSnapshot { get; }

        public IFileSystemSnapshot ToSnapshot { get; }

        public IEnumerable<IChange> Changes { get; }


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