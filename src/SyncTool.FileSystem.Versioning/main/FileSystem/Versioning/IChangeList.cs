// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace SyncTool.FileSystem.Versioning
{
    /// <summary>
    /// Groups all changes for a file
    /// </summary>
    public interface IChangeList
    {

        string Path { get; }

        IEnumerable<IChange> Changes { get; } 

    }
}