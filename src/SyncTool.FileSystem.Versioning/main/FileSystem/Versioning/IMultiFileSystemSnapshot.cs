// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace SyncTool.FileSystem.Versioning
{
    public interface IMultiFileSystemSnapshot
    {
        string Id { get; }

        IEnumerable<string> HistoryNames { get; }

        IFileSystemSnapshot GetSnapshot(string historyName);

        string GetSnapshotId(string historyName);

        /// <summary>
        /// Gets the current version of the file specified by the path from the underlying snapshots
        /// as Tuple of history name and file
        /// If a file does not exist, a tuple of (NAME, null) is returns
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        IEnumerable<Tuple<string, IFile>> GetFiles(string path);
    }
}