using System.IO;
using System.Linq;
using LibGit2Sharp;
using SyncTool.Git.Common;
using SyncTool.Utilities;

namespace SyncTool.Git.RepositoryAccess.Transactions
{
    public class CachingGitTransaction : AbstractGitTransaction
    {
        public CachingGitTransaction(string remotePath, string localPath) : base(remotePath, localPath)
        {
        }


        public override void Begin()
        {
            EnsureIsInState(TransactionState.Created);

            // if we cannot reuse the local directory, delete the directory and execute the base case            
            if (!CanReuseLocalRepository())
            {
                DirectoryHelper.DeleteRecursively(LocalPath);
                base.Begin();
                return;
            }

            // fetch all changes
            using (var repository = new Repository(LocalPath))
            {
                // TODO: Use FetchOptions.Prune once a libgit2sharp version with support for it was released 
                repository.FetchOrigin();
            }

            CreateLocalBranches();

            State = TransactionState.Active;            
        }

        /// <summary>
        /// Removes all local branches that are not tracking any remote branch
        /// </summary>
        private void RemoveRedundantBranches()
        {
            using (var repository = new Repository(this.LocalPath))
            {
                foreach (var localBranch in repository.GetLocalBranches().Where(b => !b.IsTracking))
                {
                    repository.Branches.Remove(localBranch);
                }
            }
        }

        protected override void OnTransactionCompleted()
        {
            //nop
        }

        internal bool CanReuseLocalRepository()
        {
            // if the directory does not exist, there is nothing to reuse
            if (!Directory.Exists(LocalPath))
            {
                return false;
            }

            // if the directory is empty, there is nothing to reuse
            if (!Directory.EnumerateFileSystemEntries(LocalPath).Any())
            {
                return false;
            }

            // check if the directory is a git repository
            if (!Repository.IsValid(LocalPath))
            {
                return false;
            }

            using (var repository = new Repository(LocalPath))
            {
                // check if the repository is a bare repository
                if (!repository.Info.IsBare)
                {
                    return false;
                }

                // make sure the repository has a remote named "origin" that points to RemotePath
                if (repository.Network.Remotes[s_Origin]?.Url != RemotePath)
                {
                    return false;
                }

                var localBranches = repository.GetLocalBranches().ToList();

                // make sure there are no branches that are not tracking a remote
                if (localBranches.Any(b => b.IsTracking == false))
                {
                    return false;
                }

                //fetch all branches
                repository.FetchOrigin();

                // make sure there are no unpushed changes  
                if (localBranches.Any(b => b.TrackingDetails.AheadBy > 0))
                {
                    return false;
                }
            }

            return true;
        }
    }
}