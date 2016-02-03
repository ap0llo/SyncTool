// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Linq;
using LibGit2Sharp;
using NativeDirectory = System.IO.Directory;

namespace SyncTool.Git.Common
{
    /// <summary>
    /// Provides a local (bare) copy of a repository. 
    /// It is intended to be used as a source to clone local working directories from.
    /// Commits from local working directories can be collected in this repository and then pushed to the remote repository
    /// </summary>
    public class GitTransaction
    {
        const string s_Origin = "origin";


        public bool IsActive { get; private set; }

        public string RemotePath { get; }

        public string LocalPath { get; }



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


        /// <summary>
        /// Begins a new transaction by cloning the repository and creating local branches for all remote branches.
        /// The repository will be cloned as a bare repository
        /// </summary>
        /// <exception cref="GitTransactionException">The local directory exists and is not empty</exception>        
        public void Begin()
        {
            EnsureCanCloneIntoLocalDirecotry();

            Repository.Clone(RemotePath, LocalPath, new CloneOptions { Checkout = false, IsBare = true });
            CreateLocalBranches();

            IsActive = true;
        }

        public void Commit()
        {
            using (var localRepository = new Repository(LocalPath))
            {                
                localRepository.Network.Push(localRepository.Branches.GetLocalBranches());
            }
        }




        void CheckRepository(Repository repository)
        {
            // make sure repository is a bare repository (libgit2 cannot push to it otherwise)
            if (repository.Info.IsBare == false)
            {
                throw new GitTransactionException($"The repository located at '{LocalPath}' is not a bare repository");
            }

            // make sure the remote "origin" exists and points to the remote path
            var origin = repository.Network.Remotes[s_Origin];

            if (origin == null || origin.Url.Equals(RemotePath, StringComparison.InvariantCultureIgnoreCase) == false)
            {
                throw new GitTransactionException($"The repository located at '{LocalPath}' is not a clone of '{RemotePath}'");
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




        void EnsureCanCloneIntoLocalDirecotry()
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



    }
}