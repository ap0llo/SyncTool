// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Security.Policy;

namespace SyncTool.Synchronization.ChangeGraph
{
    public interface IGraph<T>
    {
        IEnumerable<ValueNode<T>> ValueNodes { get; }
    }
}