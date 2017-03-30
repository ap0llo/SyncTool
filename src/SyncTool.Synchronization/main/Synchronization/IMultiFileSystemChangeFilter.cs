// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace SyncTool.Synchronization
{
    public interface IMultiFileSystemChangeFilter 
    {
        IEnumerable<string> HistoryNames { get; }

        IChangeFilter GetFilter(string historyName);
    }
}