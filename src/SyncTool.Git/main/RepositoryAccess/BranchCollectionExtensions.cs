using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

namespace SyncTool.Git.RepositoryAccess
{
    public static class BranchCollectionExtensions
    {
        
        public static IEnumerable<Branch> GetRemoteBranches(this BranchCollection branchCollection) => branchCollection.Where(b => b.IsRemote);

        public static IEnumerable<Branch> GetLocalBranches(this BranchCollection branchCollection) => branchCollection.Where(b => !b.IsRemote);

        public static IEnumerable<Branch> GetLocalBranchesByPrefix(this BranchCollection branchCollection, string prefix)
        {
            return branchCollection.GetLocalBranches().Where(b => BranchName.Parse(b.FriendlyName).HasPrefix(prefix));
        }


        public static IEnumerable<string> ToRefSpecs<T>(this IEnumerable<ReferenceWrapper<T>> branchCollection) where T : GitObject
        {
            return branchCollection.Select(b => b.CanonicalName);
        }
    }
}