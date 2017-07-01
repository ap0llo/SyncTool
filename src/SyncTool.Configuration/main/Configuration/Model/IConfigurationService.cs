using SyncTool.Common.Services;

namespace SyncTool.Configuration.Model
{
    /// <summary>
    /// Interface for the configuration service
    /// </summary>
    public interface IConfigurationService : IItemService<string, SyncFolder>
    {
        /// <summary>
        /// Adds the specified <see cref="SyncFolder"/> to the group
        /// </summary>
        /// <exception cref="DuplicateSyncFolderException">Thrown if the folder cannot be added because an item with the same name already exists</exception>
        void AddItem(SyncFolder folder);

        /// <summary>
        /// Updates the specified sync folder
        /// </summary>
        /// <exception cref="SyncFolderNotFoundException">Thrown when no sync folder to update could be found</exception>
        void UpdateItem(SyncFolder updatedItem);
    }
}