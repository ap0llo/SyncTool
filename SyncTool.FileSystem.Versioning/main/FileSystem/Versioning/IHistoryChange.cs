// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.FileSystem.Versioning
{
    public interface IHistoryChange : IEquatable<IHistoryChange>
    {
        string HistoryName { get; } 

        ChangeType Type { get; }
    }
}