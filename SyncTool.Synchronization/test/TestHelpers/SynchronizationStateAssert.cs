// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using SyncTool.Synchronization.State;
using Xunit;

namespace SyncTool.TestHelpers
{
    public static class SynchronizationStateAssert
    {


        public static void Equal(ISynchronizationState expected, ISynchronizationState actual)
        {
            Assert.Equal(expected.Id, actual.Id);   
            DictionaryAssert.Equal(expected.FromSnapshots, actual.FromSnapshots);
            DictionaryAssert.Equal(expected.ToSnapshots, actual.ToSnapshots);
        }



    }
}