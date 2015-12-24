// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization
{
    public class FilePropertiesComparer : IEqualityComparer<IFile>
    {
        public bool Equals(IFile x, IFile y)
        {
            if (Object.ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return StringComparer.InvariantCultureIgnoreCase.Equals(x.Path, y.Path) &&
                   x.Length == y.Length &&
                   x.LastWriteTime == y.LastWriteTime;
        }

        public int GetHashCode(IFile obj) => StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.Path);
    }
}