// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

namespace SyncTool.Git.Common
{
    public static class RepositoryExtensions
    {
        public static IEnumerable<Commit> GetAllCommits(this Repository repository)
        {
            var commits = repository.Commits.QueryBy(new CommitFilter() { IncludeReachableFrom = repository.Refs });
            return commits;
        }

        public static IEnumerable<Branch> GetRemoteBranches(this Repository repository) => repository.Branches.GetRemoteBranches();

        public static IEnumerable<Branch> GetLocalBranches(this Repository repository) => repository.Branches.GetLocalBranches();

        public static Branch GetLocalBranch(this Repository repository, string branchName)
        {
            return repository.GetLocalBranches().FirstOrDefault(b => b.FriendlyName.Equals(branchName, StringComparison.InvariantCultureIgnoreCase));
        }

    }
}