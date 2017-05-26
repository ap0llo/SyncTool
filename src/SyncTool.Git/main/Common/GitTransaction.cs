using SyncTool.Utilities;

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