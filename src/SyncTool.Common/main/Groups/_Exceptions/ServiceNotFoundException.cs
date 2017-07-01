using System;

namespace SyncTool.Common.Groups
{
    /// <summary>
    /// Indicates that a service  could not be located
    /// </summary>
    [Serializable]
    public class ServiceNotFoundException : GroupManagerException
    {

        /// <summary>
        /// Gets the type of the service that was not found
        /// </summary>
        public Type ServiceType { get; }


        public ServiceNotFoundException(Type serviceType) : base($"An service of type '{serviceType.Name}' could not be found")
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
        }

        public ServiceNotFoundException(Type serviceType, Exception innerException) : base($"An service of type '{serviceType.Name}' could not be found", innerException)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
        }
    }
}