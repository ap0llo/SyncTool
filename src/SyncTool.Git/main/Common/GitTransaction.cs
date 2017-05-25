using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using SyncTool.Common.Utilities;
using NativeDirectory = System.IO.Directory;

namespace SyncTool.Git.Common
{
    /// <summary>
    /// Provides a local (bare) copy of a repository.
    /// It is intended to be used as a source to clone local working directories from.
    /// Commits from local working directories can be collected in this repository and then pushed to the remote repository
    /// </summary>
    public class GitTransaction : AbstractGitTransaction
    {
        
        public GitTransaction(string remotePath, string localPath) : base(remotePath, localPath)
        {
        
        }

        protected override void OnTransactionCompleted() => DirectoryHelper.ClearRecursively(LocalPath);


    }
}