// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using LibGit2Sharp;
using NativeDirectory = System.IO.Directory;

namespace SyncTool.Git.Common
{
    /// <summary>
    /// Provides a local (bare) copy of a repository. 
    /// It is intended to be used as a source to clone local working directories from.
    /// Commits from local working directories can be collected in this repository and then pushed to the remote repository
    /// </summary>
    public class RepositoryClone
    {
        const string s_Origin = "origin";


        public string RemotePath { get; }

        public string LocalPath { get; }



        public RepositoryClone(string remotePath, string localPath)
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

            Initialize();
        }




        public void Push()
        {
            using (var localRepository = new Repository(LocalPath))
            {                
                localRepository.Network.Push(localRepository.Branches.GetLocalBranches());
            }
        }


        void Initialize()
        {
            // if directory does not exist, clone repository
            if (!NativeDirectory.Exists(LocalPath))
            {
                Repository.Clone(RemotePath, LocalPath, new CloneOptions { Checkout = false, IsBare = true });
                CreateLocalBranches();
            }
            // if directory exists, make sure it is a clone of the remote repository and download all changes
            else
            {                                
                using (var repository = new Repository(LocalPath))
                {
                    // check consistency
                    CheckRepository(repository);

                    // fetch changes from remote repository
                    repository.Network.Fetch(repository.Network.Remotes[s_Origin], new FetchOptions { TagFetchMode = TagFetchMode.All });      
                    
                    // make sure there is a local branch for every remote branch
                    CreateLocalBranches();              
                }
            }
        }

        void CheckRepository(Repository repository)
        {
            // make sure repository is a bare repository (libgit2 cannot push to it otherwise)
            if (repository.Info.IsBare == false)
            {
                throw new RepositoryCloneException($"The repository located at '{LocalPath}' is not a bare repository");
            }

            // make sure the remote "origin" exists and points to the remote path
            var origin = repository.Network.Remotes[s_Origin];

            if (origin == null || origin.Url.Equals(RemotePath, StringComparison.InvariantCultureIgnoreCase) == false)
            {
                throw new RepositoryCloneException($"The repository located at '{LocalPath}' is not a clone of '{RemotePath}'");
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

        



    }
}