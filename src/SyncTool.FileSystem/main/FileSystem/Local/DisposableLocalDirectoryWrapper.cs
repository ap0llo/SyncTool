﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.Common.Utilities;

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
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }
            Directory = directory;
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