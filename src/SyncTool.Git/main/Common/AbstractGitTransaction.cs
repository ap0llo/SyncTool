using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using SyncTool.Common.Utilities;

namespace SyncTool.Git.Common
{
    public abstract class AbstractGitTransaction : IGitTransaction
    {
        protected const string s_Origin = "origin";


        public TransactionState State { get; protected set; } = TransactionState.Created;

        public string RemotePath { get; }

        public string LocalPath { get; }


        protected AbstractGitTransaction(string remotePath, string localPath)
        {
            RemotePath = remotePath ?? throw new ArgumentNullException(nameof(remotePath));
            LocalPath = localPath ?? throw new ArgumentNullException(nameof(localPath));
        }

        
        public virtual void Begin()
        {
            EnsureIsInState(TransactionState.Created);

            EnsureCanCloneIntoLocalDirectory();

            try
            {
                Repository.Clone(RemotePath, LocalPath, new CloneOptions { Checkout = false, IsBare = true });
            }
            catch (LibGit2SharpException ex)
            {                
                throw new TransactionCloneException($"Could not clone from remote path '{RemotePath}'", ex);
            }
            CreateLocalBranches();

            State = TransactionState.Active;
        }

        
        public virtual void Commit()
        {
            EnsureIsInState(TransactionState.Active);

            var canCommit = CanCommit();
            if (!canCommit)
            {
                State = TransactionState.Failed;
                OnTransactionAborted();
                throw new TransactionFailedException("The changes from the transaction could not be pushed back to the remote repository because changes were made there");
            }

            try
            {
                using (var localRepository = new Repository(LocalPath))
                {
                    // push all branches with local changes and all tags
                    var branchesToPush = GetBranchesToPush(localRepository).ToList();

                    var refSpecs = branchesToPush.ToRefSpecs().Union(localRepository.Tags.ToRefSpecs());
                    localRepository.Network.Push(localRepository.Network.Remotes[s_Origin], refSpecs);
                }

                State = TransactionState.Completed;
                OnTransactionCompleted();
            }
            catch (NonFastForwardException ex)
            {
                State = TransactionState.Failed;
                OnTransactionAborted();
                throw new TransactionFailedException("The changes from the transaction could not be pushed back to the remote repository because changes were made there", ex);
            }

        }


        /// <summary>
        /// Creates local tracking branches for all remote branches
        /// </summary>
        protected void CreateLocalBranches()
        {
            using (var repository = new Repository(LocalPath))
            {
                // for every remote branch, create a local branch if it does not already exist
                foreach (var remoteBranch in repository.Branches.GetRemoteBranches())
                {
                    var branchName = remoteBranch.FriendlyName.Replace($"{s_Origin}/", "");

                    var localBranch = repository.Branches[branchName] ?? repository.CreateBranch(branchName, remoteBranch.Tip);

                    // set up the local branch to track the remote branch
                    repository.Branches.Update(localBranch, b => b.TrackedBranch = remoteBranch.CanonicalName);
                }
            }
        }

        void EnsureCanCloneIntoLocalDirectory()
        {
            // if the directory exists, make sure the directory is empty
            if (Directory.Exists(LocalPath))
            {
                if (Directory.EnumerateFiles(LocalPath).Any())
                {
                    throw new GitTransactionException($"The directory '{LocalPath}' is not empty");
                }
            }
            //if the directory does not exist, create it
            else
            {
                Directory.CreateDirectory(LocalPath);
            }
        }

        protected void EnsureIsInState(TransactionState expectedState)
        {
            if (State != expectedState)
            {
                throw new InvalidTransactionStateException(expectedState, State);
            }
        }

        bool CanCommit()
        {
            using (var localRepository = new Repository(LocalPath))
            {
                // fetch changes from the remote repository (someone might have pushed there since we cloned the repository)

                localRepository.FetchOrigin();

                var localBranches = localRepository.Branches.GetLocalBranches().ToList();

                foreach (var branch in localBranches.Where(b => b.IsTracking))
                {
                    // check if there are changes to the branch locally or in the remote branch
                    var hasRemoteChanges = branch.TrackingDetails.BehindBy > 0;
                    var hasLocalChanges = branch.TrackingDetails.AheadBy > 0;

                    //if there are both local and remote changes, we cannot continue
                    if (hasRemoteChanges && hasLocalChanges)
                    {
                        return false;
                    }
                }
            }


            return true;
        }

        IEnumerable<Branch> GetBranchesToPush(Repository localRepository)
        {
            var localBranches = localRepository.Branches.GetLocalBranches().ToList();
            
            foreach (var branch in localBranches)
            {
                if (branch.IsTracking)
                {
                    // check if there are changes to the branch locally or in the remote branch
                    var hasLocalChanges = branch.TrackingDetails.AheadBy > 0;

                    // if the branch has local changes, add it to the list of branches we need to push         
                    if (hasLocalChanges)
                    {
                        yield return branch;
                    }
                }
                else
                {
                    // set up the local branch to track the corresponding remote branch
                    localRepository.Branches.Update(
                        branch,
                        b => b.Remote = s_Origin,
                        b => b.UpstreamBranch = branch.CanonicalName);

                    yield return branch;
                }
            }            
            
        }

        protected virtual void OnTransactionAborted() => DirectoryHelper.DeleteRecursively(LocalPath);

        protected abstract void OnTransactionCompleted();
    }
}