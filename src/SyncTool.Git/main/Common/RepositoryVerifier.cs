using System.IO;
using LibGit2Sharp;
using SyncTool.Git.FileSystem;

namespace SyncTool.Git.Common
{
    public static class RepositoryVerifier
    {
        /// <summary>
        /// Verifies that the specified path points to a valid repository usable by SyncTool
        /// (a repository with the expected tags and default branches as created by RepositoryInitHelper)
        /// </summary>
        public static bool IsValid(string path)
        {
            // check if the directory exists and is a valid git repository
            if (!Directory.Exists(path) || !Repository.IsValid(path))
            {
                return false;
            }

            using (var repository = new Repository(path))
            {
                // the repository needs to be a bare repository
                if (!repository.Info.IsBare)
                {
                    return false;
                }

                // ensure there is a configuration branch
                if (!repository.LocalBranchExists(RepositoryInitHelper.ConfigurationBranchName))
                {
                    return false;
                }

                // ensure there is a tag for the initial commit
                if (repository.Tags[RepositoryInitHelper.InitialCommitTagName] == null)
                {
                    return false;
                }

                // check if there is a repository info file in the root (on all branches)
                foreach (var localBranch in repository.GetLocalBranches())
                {
                    var gitDirectory = new GitDirectory(null, "Irrelevant", localBranch.Tip);

                    if (!gitDirectory.FileExists(RepositoryInfoFile.RepositoryInfoFileName))
                    {
                        return false;
                    }

                    //TODO: verify content of repository info file
                }                
            }

            return true;

        } 
    }
}