// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace SyncTool.FileSystem.Versioning
{
    public interface IHistoryRepository
    {
                 
        IEnumerable<IFileSystemHistory> Histories { get; }

        IFileSystemHistory CreateHistory(string name);

    }
}