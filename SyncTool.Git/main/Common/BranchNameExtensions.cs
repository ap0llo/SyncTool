// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.Git.Common
{
    public static class BranchNameExtensions
    {
        public static bool HasPrefix(this BranchName branchName, string prefix) => StringComparer.InvariantCultureIgnoreCase.Equals(branchName?.Prefix, prefix);
    }
}