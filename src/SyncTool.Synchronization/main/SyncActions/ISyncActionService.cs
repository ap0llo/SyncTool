using System;
using System.Collections.Generic;
using SyncTool.Common.Services;

namespace SyncTool.Synchronization.SyncActions
{
    public interface ISyncActionService : IService
    {
        //TODO: Rename to 'Items' to make naming consistent with IItemService
        IEnumerable<SyncAction> AllItems { get; }

        IEnumerable<SyncAction> PendingItems { get; }

            /// <summary>
        /// Gets all sync actions with the specified state
        /// </summary>
        IEnumerable<SyncAction> this[SyncActionState state] { get; }

        /// <summary>
        /// Gets all sync actions for the specified state and file path
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null</exception>
        /// <exception cref="FormatException"><paramref name="filePath"/> is not a valid file path</exception>
        IEnumerable<SyncAction> this[SyncActionState state, string filePath] { get; }

        /// <summary>
        /// Gets all sync actions for the specified file path
        /// </summary>
        IEnumerable<SyncAction> this[string filePath] { get; }


        /// <summary>
        /// Adds the specified sync actions to the service
        /// </summary>
        /// <exception cref="DuplicateSyncActionException"><paramref name="syncActions"/> contains a action with a id that already exists</exception>
        void AddItems(IEnumerable<SyncAction> syncActions);

        /// <summary>
        /// Updates the specified sync actions
        /// </summary>
        /// <exception cref="SyncActionNotFoundException"><paramref name="syncActions"/> contains an action with an id that does not yet exist</exception>
        void UpdateItems(IEnumerable<SyncAction> syncActions);

        /// <summary>
        /// Removes the specified sync actions
        /// </summary>
        /// <exception cref="SyncActionNotFoundException"><paramref name="syncActions"/> contains an action that does not exist</exception>
        void RemoveItems(IEnumerable<SyncAction> syncActions);
        
    }
}