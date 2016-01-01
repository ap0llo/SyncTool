// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace SyncTool.Common
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
        T GetService<T>() where T : IService;
    }

}