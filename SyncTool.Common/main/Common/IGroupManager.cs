// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace SyncTool.Common
{
    public interface IGroupManager
    {
        /// <summary>
        /// Gets the names of all groups managed by the group mananger
        /// </summary>
        IEnumerable<string> Groups { get; }

        /// <summary>
        /// Gets the specified group
        /// </summary>
        /// <param name="name">The name of the group</param>
        /// <remarks>Group names are case-invariant</remarks>
        /// <exception cref="GroupNotFoundException">The specified group could not be found</exception>
        IGroup GetGroup(string name);

        /// <summary>
        /// Adds a new group
        /// </summary>
        /// <param name="name">The name of the group to add</param>
        /// <remarks>Group names are case-invariant</remarks>
        /// <exception cref="DuplicateGroupException">Thrown if a group with the specified name already exists</exception>
        void AddGroup(string name);

        /// <summary>
        /// Removes the specified group
        /// </summary>
        /// <param name="name">The name of the group to remove</param>
        /// <remarks>Group names are case-invariant</remarks>
        /// <exception cref="GroupNotFoundException">The specified group could not be found</exception>
        void RemoveGroup(string name);
    }
}