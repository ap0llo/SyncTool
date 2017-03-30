// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using SyncTool.Configuration.Model;

namespace SyncTool.Synchronization
{
    public interface IChangeFilterFactory
    {
        IChangeFilter GetFilter(FilterConfiguration configuration);
    }
}