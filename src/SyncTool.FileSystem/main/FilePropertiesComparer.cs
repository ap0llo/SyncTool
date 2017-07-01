using System;
using System.Collections.Generic;

namespace SyncTool.FileSystem
{
    /// <summary>
    ///  IEqualityComparer{IFile} that only considers the file's properties (Path, Length, LastWriteTime)
    /// </summary>
    public class FilePropertiesComparer : IEqualityComparer<IFile>
    {
        public bool Equals(IFile x, IFile y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x == null || y == null)
                return false;

            return StringComparer.InvariantCultureIgnoreCase.Equals(x.Path, y.Path) &&
                   x.Length == y.Length &&
                   x.LastWriteTime == y.LastWriteTime;
        }

        public int GetHashCode(IFile obj) => StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.Path);
    }
}