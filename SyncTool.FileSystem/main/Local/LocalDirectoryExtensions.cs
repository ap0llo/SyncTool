// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
namespace SyncTool.FileSystem.Local
{
    public static class LocalDirectoryExtensions
    {
        
        public static TemporaryLocalDirectory ToTemporaryDirectory(this ILocalDirectory directory) => new TemporaryLocalDirectory(directory);
    }
}