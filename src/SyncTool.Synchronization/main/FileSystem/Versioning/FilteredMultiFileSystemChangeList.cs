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
    public class FilteredMultiFileSystemChangeList : IMultiFileSystemChangeList
    {
        readonly IMultiFileSystemChangeList m_InnerList;
        readonly IMultiFileSystemChangeFilter m_Filter;


        public string Path => m_InnerList.Path;

        public IEnumerable<string> HistoryNames => m_InnerList.HistoryNames;

        public IEnumerable<IChange> AllChanges => HistoryNames.SelectMany(GetChanges);


        public FilteredMultiFileSystemChangeList(IMultiFileSystemChangeList innerList, IMultiFileSystemChangeFilter filter)
        {
            if (innerList == null)
                throw new ArgumentNullException(nameof(innerList));

            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            m_InnerList = innerList;
            m_Filter = filter;
        }
        

        public IEnumerable<IChange> GetChanges(string historyName)
        {
            return m_InnerList.GetChanges(historyName).Where(m_Filter.GetFilter(historyName).IncludeInResult);
        }
    }
}