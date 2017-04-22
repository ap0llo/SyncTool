using System.Collections.Generic;

namespace SyncTool.Common.Utilities
{
    public interface IReversibleDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        /// <summary>
        ///     Gets the reversed dictionary that contains the same items but with key and value swapped
        /// </summary>
        IReversibleDictionary<TValue, TKey> ReversedDictionary { get; }
    }
}