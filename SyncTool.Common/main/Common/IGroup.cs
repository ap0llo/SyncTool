// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace SyncTool.Common
{

    public interface IGroup
    {
        string Name { get; }

    }

    public interface IGroup<T> : IGroup
    {        
        IEnumerable<T> Items { get; }

        T GetItem(string name);
    }
}