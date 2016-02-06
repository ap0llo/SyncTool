// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
namespace SyncTool.Git.Common
{
    public class CachingGitTransaction : GitTransaction
    {
        public CachingGitTransaction(string remotePath, string localPath) : base(remotePath, localPath)
        {
        }
    }
}