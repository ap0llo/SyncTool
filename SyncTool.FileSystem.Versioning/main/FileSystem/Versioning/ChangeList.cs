// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.FileSystem.Versioning
{
    /// <summary>
    /// Default, immutable implementation of <see cref="IChangeList"/>
    /// </summary>
    public class ChangeList : IChangeList
    {
        public string Path { get; }

        public IEnumerable<IChange> Changes { get; }


        public ChangeList(IEnumerable<IChange> changes)
        {
            if (changes == null)
            {
                throw new ArgumentNullException(nameof(changes));
            }

            changes = changes.ToList();

            if (!changes.Any())
            {
                throw new ArgumentException("ChangeList must not be empty", nameof(changes));
            }

            var pathCount = changes.Select(c => c.Path).Distinct().Count();
            if (pathCount != 1)
            {
                throw new ArgumentException("The changes in the change list must not refer to different file paths");
            }

            Path = changes.First().Path;
            Changes = changes;
        }

        

    }
}