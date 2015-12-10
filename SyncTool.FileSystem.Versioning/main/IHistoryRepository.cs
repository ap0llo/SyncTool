// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace SyncTool.FileSystem.Versioning
{
    public interface IHistoryRepository : IDisposable
    {        
        string Name { get; }

        IEnumerable<IFileSystemHistory> Histories { get; }

        void CreateHistory(string name);

        IFileSystemHistory GetHistory(string name);


    }
}