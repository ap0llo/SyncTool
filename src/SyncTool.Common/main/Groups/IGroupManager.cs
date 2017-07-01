using System.Collections.Generic;

namespace SyncTool.Common.Groups
{
    public interface IGroupManager
    {
        /// <summary>
        /// Gets the names of all groups managed by the group mananger
        /// </summary>
        IEnumerable<string> Groups { get; }

        IGroup OpenShared(string name);

        IGroup OpenExclusively(string name);

        /// <summary>
        /// Adds an existing group
        /// </summary>
        /// <param name="name">The name of the group</param>
        /// <param name="address">The address of the group</param>
        /// <remarks>Group names are case-invariant</remarks>
        /// <exception cref="DuplicateGroupException">Thrown if a group with the specified name already exists</exception>
        void AddGroup(string name, string address);

        /// <summary>
        /// Creates a new group
        /// </summary>
        /// <param name="name">The name of the group to create</param>
        /// <param name="address">The address of the group</param>
        /// <remarks>Group names are case-invariant</remarks>
        /// <exception cref="DuplicateGroupException">Thrown if a group with the specified name already exists</exception>
        void CreateGroup(string name, string address);

        /// <summary>
        /// Removes the specified group
        /// </summary>
        /// <param name="name">The name of the group to remove</param>
        /// <remarks>Group names are case-invariant</remarks>
        /// <exception cref="GroupNotFoundException">The specified group could not be found</exception>
        void RemoveGroup(string name);
    }
}