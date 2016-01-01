// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.Common
{
    /// <summary>
    /// Indicates that a service  could not be located
    /// </summary>
    [Serializable]
    public class ServiceNotFoundException : Exception
    {

        /// <summary>
        /// Gets the type of the service that was not found
        /// </summary>
        public Type ServiceType { get; }


        public ServiceNotFoundException(Type serviceType) : base($"An service of type '{serviceType.Name}' could not be found")
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            this.ServiceType = serviceType;
        }
    }
}