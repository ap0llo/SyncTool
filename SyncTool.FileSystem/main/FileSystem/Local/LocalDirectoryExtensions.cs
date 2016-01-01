// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
namespace SyncTool.FileSystem.Local
{
    public static class LocalDirectoryExtensions
    {
        /// <summary>
        /// Wraps the local directory into a disposable wrapper
        /// that deletes the directory when it is disposed
        /// </summary>
        public static DisposableLocalDirectoryWrapper ToTemporaryDirectory(this ILocalDirectory directory) => new DisposableLocalDirectoryWrapper(directory);
    }
}