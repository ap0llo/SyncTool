// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace SyncTool.Common
{
    /// <summary>
    /// Interface for a service managing items of type <typeparam name="T" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IItemService<T> : IService
    {
        /// <summary>
        /// Gets all the items managed by the service
        /// </summary>
        IEnumerable<T> Items { get; }

        /// <summary>
        /// Gets the specified item from the group
        /// </summary>
        /// <param name="name">The name of the item to retrieve</param>
        /// <returns>Returns the requested item</returns>
        /// <exception cref="ItemNotFoundException">Thrown if the specified item could not be found</exception>
        /// <exception cref="ArgumentNullException">Thrown if 'name' is null or empty</exception>
        T this[string name] { get; }

        /// <summary>
        /// Checks whether an item with the specified name exists
        /// </summary>
        bool ItemExists(string name);
    }
}