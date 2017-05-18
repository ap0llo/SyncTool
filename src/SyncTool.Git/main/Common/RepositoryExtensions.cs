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

        public static Branch GetLocalBranch(this Repository repository, BranchName branchName)
        {
            return repository.GetLocalBranches().FirstOrDefault(b => b.FriendlyName.Equals(branchName.ToString(), StringComparison.InvariantCultureIgnoreCase));
        }


        public static bool LocalBranchExists(this Repository repository, string branchName)
        {
            return repository.GetLocalBranches().Any(b => b.FriendlyName.Equals(branchName, StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool LocalBranchExists(this Repository repository, BranchName branchName) => repository.LocalBranchExists(branchName.ToString());

        public static Commit GetInitialCommit(this Repository repository)
        {
            var sha = repository.Tags[RepositoryInitHelper.InitialCommitTagName].Target.Sha;
            return repository.Lookup<Commit>(sha);
        }


        public static Branch CreateBranch(this Repository repository, BranchName branchName, Commit commit) => repository.CreateBranch(branchName.ToString(), commit);

        public static Branch GetBranch(this Repository repository, BranchName branchName) => repository.Branches.FirstOrDefault(b => BranchName.Parse(b.FriendlyName).Equals(branchName));


        public static bool IsCommitAncestor(this Repository repository, string ancestorId, string descandantId)
        {
            var ancestor = repository.Lookup<Commit>(ancestorId);
            var descandant = repository.Lookup<Commit>(descandantId);

            var mergeBase = repository.ObjectDatabase.FindMergeBase(ancestor, descandant);

            return mergeBase != null && mergeBase.Sha == ancestor.Sha;
        }

        public static void FetchOrigin(this Repository repository)
        {
            var remote = repository.Network.Remotes["origin"];
            Commands.Fetch(
                repository,
                "origin",
                remote.FetchRefSpecs.Select(x => x.Specification),
                null, //new FetchOptions() { Prune = true },
                "");

        }

    }
}