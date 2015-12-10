// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using SyncTool.Common;

namespace SyncTool.FileSystem.Versioning
{
    public interface IHistoryGroup : IGroup<IFileSystemHistory>, IDisposable
    {        
        
        void CreateHistory(string name);
        

    }
}