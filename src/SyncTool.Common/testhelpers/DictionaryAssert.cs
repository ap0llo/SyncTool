using System.Collections.Generic;
using Xunit;

namespace SyncTool.Common.TestHelpers
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