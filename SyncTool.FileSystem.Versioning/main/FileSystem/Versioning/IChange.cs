// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

namespace SyncTool.FileSystem.Versioning
{
    public interface IChange
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
        /// The file before the change (Not available for changes of type 'Addded')
        /// </summary>
        IFile FromFile { get; }

        /// <summary>
        /// The file after the change (Not available for changes of type 'Deleted')
        /// </summary>
        IFile ToFile { get; }
    }
}