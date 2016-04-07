// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Security.Cryptography.X509Certificates;
using SyncTool.Common;

namespace SyncTool.Synchronization.State
{
    public interface ISyncPointService : IItemService<int, ISyncPoint>
    {
        /// <summary>
        /// Gets the latest sync point (the point with the highest id) or null if no sync point exists
        /// </summary>
        ISyncPoint LatestSyncPoint { get; }

        //TODO: check that new item has a valid id (LatestSyncPoint.Id + 1)
        /// <summary>
        /// Adds a new synchronization state
        /// </summary>
        /// <param name="state">The state to add</param>
        /// <exception cref="DuplicateSyncPointException">Thrown if a state with the same id already exists</exception>
        void AddItem(ISyncPoint state);



    }
}