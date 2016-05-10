// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.FileSystem.Versioning;
using SyncTool.Synchronization.SyncActions;

namespace SyncTool.Synchronization
{
    public interface IChangeFilter : IEquatable<IChangeFilter>
    {
        bool IncludeInResult(IChangeList changeList);

        bool IncludeInResult(IChange change);
    }
}