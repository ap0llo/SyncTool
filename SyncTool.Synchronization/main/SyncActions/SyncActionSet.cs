// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;

namespace SyncTool.Synchronization
{
    public class SyncActionSet : IEnumerable<SyncAction>
    {         
        readonly LinkedList<SyncAction> m_Actions = new LinkedList<SyncAction>();

        public void Add(SyncAction action)
        {
            m_Actions.AddLast(action);
        }

        public IEnumerator<SyncAction> GetEnumerator() => m_Actions.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}