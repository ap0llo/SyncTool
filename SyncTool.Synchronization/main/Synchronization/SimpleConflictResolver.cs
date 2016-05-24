// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization
{
    /// <summary>
    /// A implementation of <see cref="IConflictResolver"/> that resolves conflicts
    /// that have "disappeared" on their own (file versions causing conflicts might have been replaced in the file system)
    /// </summary>
    public class SimpleConflictResolver : ConflictResolverBase
    {
        public SimpleConflictResolver(IEqualityComparer<IFileReference> fileReferenceComparer) : base(fileReferenceComparer)
        {
        }


        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        protected override bool TryResolveConflict(IEnumerable<IFileReference> versions, out IFileReference resolvedVersion)
        {
            var containsMultipleItems = versions.Skip(1).Any();

            if (containsMultipleItems)
            {
                resolvedVersion = null;
                return false;
            }
            else
            {
                resolvedVersion = versions.Single();
                return true;
            }
        }
    }
}