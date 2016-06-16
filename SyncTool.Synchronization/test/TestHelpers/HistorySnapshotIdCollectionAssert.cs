// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using SyncTool.Synchronization.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SyncTool.TestHelpers
{
    public static class HistorySnapshotIdCollectionAssert
    {
        public static void Equal(HistorySnapshotIdCollection expected, HistorySnapshotIdCollection actual)
        {
            Assert.Empty(expected.Except(actual));
        }


    }
}
