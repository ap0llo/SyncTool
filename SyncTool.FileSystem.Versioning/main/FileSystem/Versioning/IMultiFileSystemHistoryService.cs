// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using SyncTool.Common;

namespace SyncTool.FileSystem.Versioning
{
    public interface IMultiFileSystemHistoryService : IService
    {         
        IMultiFileSystemSnapshot LatestSnapshot { get; }
    
        IEnumerable<IMultiFileSystemSnapshot> Snapshots { get; }

        IMultiFileSystemSnapshot this[string id] { get; }

        IMultiFileSystemSnapshot CreateSnapshot();

        string[] GetChangedFiles(string to);

        string[] GetChangedFiles(string from, string to);

    }
}