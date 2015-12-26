// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.Common;

namespace SyncTool.Configuration.Model
{
    public interface IConfigurationGroup : IGroup<SyncFolder>, IDisposable
    {        

        void AddSyncFolder(SyncFolder folder);                

    }
}