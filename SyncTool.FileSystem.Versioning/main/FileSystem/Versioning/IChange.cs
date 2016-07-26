// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.FileSystem.Versioning
{
    public interface IChange : IEquatable<IChange>
    {
        /// <summary>
        /// The path of the file that was changed
        /// </summary>
        string Path { get; }

        /// <summary>
        /// The type of the change
        /// </summary>
        ChangeType Type { get; }

        /// <summary>
        /// A reference to the file before the modification
        /// </summary>
        IFileReference FromVersion { get; }

        /// <summary>
        /// A reference to the file after the modification
        /// </summary>
        IFileReference ToVersion { get; }
    }
}