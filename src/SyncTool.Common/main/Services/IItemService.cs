using System;
using System.Collections.Generic;

namespace SyncTool.Common.Services
{
    /// <summary>
    /// Interface for a service managing items of type <typeparam name="TValue" /> identified by keys of type <typeparam name="TKey"/>
    /// </summary>
    public interface IItemService<in TKey, out TValue> : IService
    {
        /// <summary>
        /// Gets all the items managed by the service
        /// </summary>
        IEnumerable<TValue> Items { get; }

        /// <summary>
        /// Gets the specified item from the group
        /// </summary>
        /// <param name="key">The key of the item to retrieve</param>
        /// <returns>Returns the requested item</returns>
        /// <exception cref="ItemNotFoundException">Thrown if the specified item could not be found</exception>
        /// <exception cref="ArgumentNullException">Thrown if 'name' is null or empty</exception>
        TValue this[TKey key] { get; }

        /// <summary>
        /// Checks whether an item with the specified name exists
        /// </summary>
        bool ItemExists(TKey key);
    }
}