// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using SyncTool.Common;

namespace SyncTool.Configuration.Model
{
    /// <summary>
    /// Interface for the configuration service
    /// </summary>
    public interface IConfigurationService : IItemService<SyncFolder>
    {
        /// <summary>
        /// Adds the specified <see cref="SyncFolder"/> to the group
        /// </summary>
        /// <exception cref="DuplicateSyncFolderException">Thrown if the folder cannot be added because an item with the same name already exists</exception>
        void AddSyncFolder(SyncFolder folder);        
    }
}