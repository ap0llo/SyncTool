// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace SyncTool.Utilities
{
    public interface IReversibleDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        /// <summary>
        ///     Gets the reversed dictionary that contains the same items but with key and value swapped
        /// </summary>
        IReversibleDictionary<TValue, TKey> ReversedDictionary { get; }
    }
}