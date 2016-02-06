// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
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
        


        public override TransactionState State { get; protected set; } = TransactionState.Created;

        public override string RemotePath { get; }

        public override string LocalPath { get; }



        public GitTransaction(string remotePath, string localPath)
        {
            if (remotePath == null)
            {
                throw new ArgumentNullException(nameof(remotePath));
            }
            if (localPath == null)
            {
                throw new ArgumentNullException(nameof(localPath));
            }

            RemotePath = remotePath;
            LocalPath = localPath;
        }


        
        public override void Begin()
        {
            EnsureIsInState(TransactionState.Created);

            EnsureCanCloneIntoLocalDirectory();

            Repository.Clone(RemotePath, LocalPath, new CloneOptions {Checkout = false, IsBare = true});
            CreateLocalBranches();

            State = TransactionState.Active;
        }
        
        public override void Commit()
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
                    // push all branches with local changes
                    var branchesToPush = GetBranchesToPush(localRepository);
                    localRepository.Network.Push(branchesToPush);
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
        void CreateLocalBranches()
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
            if (NativeDirectory.Exists(LocalPath))
            {
                if (NativeDirectory.EnumerateFiles(LocalPath).Any())
                {
                    throw new GitTransactionException($"The directory '{LocalPath}' is not empty");
                }
            }
            //if the directory does not exist, create it
            else
            {
                NativeDirectory.CreateDirectory(LocalPath);
            }
        }

        void EnsureIsInState(TransactionState expectedState)
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
                localRepository.Network.Fetch(localRepository.Network.Remotes[s_Origin]);

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

            var branchesToPush = new LinkedList<Branch>();
            foreach (var branch in localBranches)
            {
                if (branch.IsTracking)
                {
                    // check if there are changes to the branch locally or in the remote branch
                    var hasLocalChanges = branch.TrackingDetails.AheadBy > 0;

                    // if the branch has local changes, add it to the list of branches we need to push         
                    if (hasLocalChanges)
                    {
                        branchesToPush.AddLast(branch);
                    }
                }
                else
                {
                    localRepository.Branches.Update(
                        branch,
                        b => b.Remote = s_Origin,
                        b => b.UpstreamBranch = branch.CanonicalName);

                    branchesToPush.AddLast(branch);
                }
            }

            return branchesToPush;
        }


        protected virtual void OnTransactionAborted() => DirectoryHelper.DeleteRecursively(LocalPath);

        protected virtual void OnTransactionCompleted() => DirectoryHelper.DeleteRecursively(LocalPath);
    }
}