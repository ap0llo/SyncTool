using System;
using SyncTool.Utilities;

namespace SyncTool.FileSystem.Local
{
    /// <summary>
    /// Wraps any instance of <see cref="ILocalDirectory"/> and deletes it when the wrapper is disposed
    /// </summary>    
    public sealed class DisposableLocalDirectoryWrapper : IDisposable
    {
        /// <summary>
        /// Gets the current state of the directory
        /// </summary>
        public ILocalDirectory Directory { get; }

        /// <summary>
        /// Gets the location of the temporary directory in the local filesystem
        /// </summary>
        public string Location => Directory.Location;


        /// <summary>
        /// Initializes a new instance of <see cref="DisposableLocalDirectoryWrapper"/>
        /// </summary>
        /// <param name="directory">The local directory to wrao</param>
        public DisposableLocalDirectoryWrapper(ILocalDirectory directory)
        {
            Directory = directory ?? throw new ArgumentNullException(nameof(directory));
        }


        /// <summary>
        /// Deletes the wrapped directory
        /// </summary>
        public void Dispose()
        {
            DirectoryHelper.DeleteRecursively(Directory.Location);
        }

    
    }
}