// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace SyncTool.Synchronization
{
    public class MultiFileSystemChangeFilter : IMultiFileSystemChangeFilter
    {
        readonly IDictionary<string, IChangeFilter> m_Filters;


        public IEnumerable<string> HistoryNames => m_Filters.Keys;


        public MultiFileSystemChangeFilter(IEnumerable<Tuple<string, IChangeFilter>> filters)
        {
            if (filters == null)
                throw new ArgumentNullException(nameof(filters));

            m_Filters = filters.ToDictionary(
                keySelector: tuple => tuple.Item1,
                elementSelector: tuple => tuple.Item2,
                comparer: StringComparer.InvariantCultureIgnoreCase
                );
        }

        public IChangeFilter GetFilter(string historyName) => m_Filters[historyName];
    }
}