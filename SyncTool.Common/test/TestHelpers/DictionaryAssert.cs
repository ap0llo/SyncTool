// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace SyncTool.TestHelpers
{
    public static class DictionaryAssert
    {
        public static void Equal<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> expected, IReadOnlyDictionary<TKey, TValue> actual)
        {
            Assert.Equal(expected?.Keys, actual?.Keys);
            Assert.Equal(expected?.Values, actual?.Values);                          
        }

    }
}