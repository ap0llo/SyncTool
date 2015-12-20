// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.FileSystem.Versioning
{
    public interface IFileSystemDiff
    {
         
        IFileSystemSnapshot FromSnapshot { get; }

        IFileSystemSnapshot ToSnapshot { get; }


        IEnumerable<IChange> Changes { get; } 

    }
}