// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;

namespace SyncTool.Common.Utilities
{

	public class ReversibleDictionary<TKey, TValue> : IReversibleDictionary<TKey, TValue>
	{

		#region Fields

		readonly IDictionary<TKey, TValue> m_KeyDictionary = new Dictionary<TKey, TValue>();
		readonly IDictionary<TValue, TKey> m_ValueDictionary = new Dictionary<TValue, TKey>();

		#endregion


		#region Constructor

		public ReversibleDictionary()
		{
			ReversedDictionary = new ReversedDictionaryImplementation(this);
		}

		#endregion


		#region IBidirectionalDictionary Implementation	

		public IReversibleDictionary<TValue, TKey> ReversedDictionary { get; }

		#endregion


		#region IDictionary Implementation

		public TValue this[TKey key]
		{
			get
			{
				return m_KeyDictionary[key];
			}
			set
			{
				var reversedKey = m_KeyDictionary[key];	//throws correct exception is value cannot be found

				m_KeyDictionary[key] = value;
				lock (this)
				{
					m_ValueDictionary.Remove(reversedKey);
					m_ValueDictionary.Add(value, key);
				}

			}
		}

		public int Count => m_KeyDictionary.Count;

		public bool IsReadOnly => false;

		public ICollection<TKey> Keys => m_KeyDictionary.Keys;

		public ICollection<TValue> Values => m_KeyDictionary.Values;

		public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

		public void Add(TKey key, TValue value)
		{
			lock (this)
			{
				if (m_KeyDictionary.ContainsKey(key) || m_ValueDictionary.ContainsKey(value))
				{
					throw new ArgumentException("A item with the same key already exists");
				}

				m_KeyDictionary.Add(key, value);
				m_ValueDictionary.Add(value, key);
			}
		}

		public void Clear()
		{
			lock (this)
			{
				m_KeyDictionary.Clear();
				m_ValueDictionary.Clear();
			}
		}

		public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);

		public bool ContainsKey(TKey key) => m_KeyDictionary.ContainsKey(key);

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => m_KeyDictionary.CopyTo(array, arrayIndex);

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => m_KeyDictionary.GetEnumerator();

		public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

		public bool Remove(TKey key)
		{
			lock (this)
			{
				if (m_KeyDictionary.ContainsKey(key))
				{
					var reverseKey = m_KeyDictionary[key];
					m_KeyDictionary.Remove(key);
					m_ValueDictionary.Remove(reverseKey);
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public bool TryGetValue(TKey key, out TValue value) => m_KeyDictionary.TryGetValue(key, out value);

		IEnumerator IEnumerable.GetEnumerator() => m_KeyDictionary.GetEnumerator();

		#endregion


		#region Private Types

		private class ReversedDictionaryImplementation : IReversibleDictionary<TValue, TKey>
		{
			readonly ReversibleDictionary<TKey, TValue> m_Parent;

			public ReversedDictionaryImplementation(ReversibleDictionary<TKey, TValue> parent)
			{
				m_Parent = parent;
			}

			public TKey this[TValue key]
			{
				get
				{
					return m_Parent.m_ValueDictionary[key];
				}
				set
				{
					var oldValue = m_Parent.m_ValueDictionary[key];
					m_Parent.m_ValueDictionary[key] = value;
					m_Parent.m_KeyDictionary.Remove(oldValue);
					m_Parent.m_KeyDictionary.Add(value, key);
				}
			}

			public int Count => m_Parent.Count;

			public bool IsReadOnly => m_Parent.IsReadOnly;

			public ICollection<TValue> Keys => m_Parent.Values;

			public ICollection<TKey> Values => m_Parent.Keys;

			IReversibleDictionary<TKey, TValue> IReversibleDictionary<TValue, TKey>.ReversedDictionary => m_Parent;

			public void Add(KeyValuePair<TValue, TKey> item) => Add(item.Key, item.Value);

			public void Add(TValue key, TKey value) => m_Parent.Add(value, key);

			public void Clear() => m_Parent.Clear();

			public bool Contains(KeyValuePair<TValue, TKey> item) => ContainsKey(item.Key) && m_Parent.ContainsKey(item.Value);

			public bool ContainsKey(TValue key) => m_Parent.m_ValueDictionary.ContainsKey(key);

			public void CopyTo(KeyValuePair<TValue, TKey>[] array, int arrayIndex) => m_Parent.m_ValueDictionary.CopyTo(array, arrayIndex);

			public IEnumerator<KeyValuePair<TValue, TKey>> GetEnumerator() => m_Parent.m_ValueDictionary.GetEnumerator();

			public bool Remove(KeyValuePair<TValue, TKey> item) => ContainsKey(item.Key) && Remove(item.Key);

			public bool Remove(TValue key) => ContainsKey(key) && m_Parent.Remove(this[key]);

			public bool TryGetValue(TValue key, out TKey value) => m_Parent.m_ValueDictionary.TryGetValue(key, out value);

			IEnumerator IEnumerable.GetEnumerator() => m_Parent.m_ValueDictionary.GetEnumerator();

		}

		#endregion

	}
}
