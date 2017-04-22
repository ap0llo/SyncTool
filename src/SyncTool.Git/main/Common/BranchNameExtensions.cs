using System;

namespace SyncTool.Git.Common
{
    public static class BranchNameExtensions
    {
        public static bool HasPrefix(this BranchName branchName, string prefix) => StringComparer.InvariantCultureIgnoreCase.Equals(branchName?.Prefix, prefix);
    }
}