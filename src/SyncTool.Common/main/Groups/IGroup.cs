﻿using System;

namespace SyncTool.Common.Groups
{
    /// <summary>
    /// Interface for interacting with a group
    /// </summary>
    public interface IGroup : IDisposable
    {
        /// <summary>
        /// Gets the name of the group
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a service for the group
        /// </summary>
        /// <typeparam name="T">The type of the service to retrieve</typeparam>
        /// <exception cref="ServiceNotFoundException">Thrown if a service of the specified type could not be found</exception>
        T GetService<T>();
    }

}