// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace SyncTool.Utilities
{
    public interface IObjectMapper<TSource, TTarget>
    {
        TTarget MapObject(TSource item);
      
    }
}