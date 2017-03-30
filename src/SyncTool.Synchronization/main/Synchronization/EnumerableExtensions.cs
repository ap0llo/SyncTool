// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using SyncTool.Configuration.Model;

namespace SyncTool.Synchronization
{
    public static class EnumerableExtensions
    {

        public static IMultiFileSystemChangeFilter ToMultiFileSystemChangeFilter(this IEnumerable<SyncFolder> syncFolders, IChangeFilterFactory filterFactory)
        {
            var tuples = from syncFolder in syncFolders
                let filter = filterFactory.GetFilter(syncFolder.Filter)
                select new Tuple<string, IChangeFilter>(syncFolder.Name, filter);

            return new MultiFileSystemChangeFilter(tuples);

        }

    }
}