// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

namespace SyncTool.FileSystem.Git
{
    public static class BranchCollectionExtensions
    {
        
        public static IEnumerable<Branch> GetRemoteBranches(this BranchCollection branchCollection) => branchCollection.Where(b => b.IsRemote);

        public static IEnumerable<Branch> GetLocalBranches(this BranchCollection branchCollection) => branchCollection.Where(b => !b.IsRemote);
    }
}