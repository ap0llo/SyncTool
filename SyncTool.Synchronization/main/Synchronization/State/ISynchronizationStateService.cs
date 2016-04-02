// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Security.Cryptography.X509Certificates;
using SyncTool.Common;

namespace SyncTool.Synchronization.State
{
    public interface ISynchronizationStateService : IItemService<int, ISynchronizationState>
    {

        /// <summary>
        /// Adds a new synchronization state
        /// </summary>
        /// <param name="state">The state to add</param>
        /// <exception cref="DuplicateSynchronizationStateException">Thrown if a state with the same id already exists</exception>
        void AddSynchronizationState(ISynchronizationState state);



    }
}