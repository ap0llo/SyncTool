// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using SyncTool.Common;

namespace SyncTool.Synchronization.SyncActions
{
    public interface ISyncActionService : IService
    {        
        void Add(IEnumerable<SyncAction> syncActions);

        void Update(IEnumerable<SyncAction> syncActions);

        void Remove(IEnumerable<SyncAction> syncActions);

        IEnumerable<SyncAction> this[SyncActionState state] { get; }

        IEnumerable<SyncAction> this[SyncActionState state, string filePath] { get; }
        
    }
}