// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using SyncTool.Synchronization;

namespace SyncTool.FileSystem.Versioning
{
    public class FilteredMultiFileSystemDiff : IMultiFileSystemDiff
    {
        readonly IMultiFileSystemDiff m_InnerDiff;
        readonly IMultiFileSystemChangeFilter m_Filter;

        public FilteredMultiFileSystemDiff(IMultiFileSystemDiff innerDiff, IMultiFileSystemChangeFilter filter)
        {
            if (innerDiff == null)
                throw new ArgumentNullException(nameof(innerDiff));

            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            m_InnerDiff = innerDiff;
            m_Filter = filter;
        }

        public IMultiFileSystemSnapshot FromSnapshot => m_InnerDiff.FromSnapshot;

        public IMultiFileSystemSnapshot ToSnapshot => m_InnerDiff.ToSnapshot;

        public IEnumerable<IMultiFileSystemChangeList> FileChanges => FilterChanges(m_InnerDiff.FileChanges);

        public IEnumerable<IHistoryChange> HistoryChanges => m_InnerDiff.HistoryChanges;


        IEnumerable<IMultiFileSystemChangeList> FilterChanges(IEnumerable<IMultiFileSystemChangeList> changes)
        {
            foreach (var changeList in changes)
            {
                // create a filtered change list
                var filteredList = new FilteredMultiFileSystemChangeList(changeList, m_Filter);

                // if change list is empty after filtering, do not include the change list in the result at all
                if (filteredList.AllChanges.Any())
                    yield return filteredList;
            }
        }
    }
}