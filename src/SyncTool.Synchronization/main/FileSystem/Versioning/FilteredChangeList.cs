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
    public class FilteredChangeList : IChangeList
    {

        readonly IChangeList m_WrappedList;
        readonly IChangeFilter m_Filter;

        public FilteredChangeList(IChangeList wrappedList, IChangeFilter filter)
        {
            if (wrappedList == null)
            {
                throw new ArgumentNullException(nameof(wrappedList));
            }
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }
            m_WrappedList = wrappedList;
            m_Filter = filter;
        }


        public string Path => m_WrappedList.Path;

        public IEnumerable<IChange> Changes => m_WrappedList.Changes.Where(m_Filter.IncludeInResult);
    }
}