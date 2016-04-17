// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Synchronization
{
    internal class ChangeListWithHistoryName : IChangeList
    {
        readonly IChangeList m_WrappedChangeList;


        public string HistoryName { get; }

        public string Path => m_WrappedChangeList.Path;

        public IEnumerable<IChange> Changes => m_WrappedChangeList.Changes;


        public ChangeListWithHistoryName(string historyName, IChangeList wrappedChangeList)
        {
            if (historyName == null)
            {
                throw new ArgumentNullException(nameof(historyName));
            }
            if (wrappedChangeList == null)
            {
                throw new ArgumentNullException(nameof(wrappedChangeList));
            }
            HistoryName = historyName;
            m_WrappedChangeList = wrappedChangeList;
        }

        
    }
}