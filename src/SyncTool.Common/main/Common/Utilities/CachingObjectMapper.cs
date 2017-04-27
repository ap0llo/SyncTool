using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.Common.Utilities
{
    public sealed class CachingObjectMapper<TSource, TTarget> : IObjectMapper<TSource, TTarget>, IDisposable
    {
        readonly Func<TSource, TTarget> m_MappingFunction;
        readonly IEqualityComparer<TSource> m_EqualityComparer;
        readonly IDictionary<TSource, TTarget> m_Cache;


        public CachingObjectMapper(Func<TSource, TTarget> mappingFunction) : this(mappingFunction, EqualityComparer<TSource>.Default)            
        {            
        }

        public CachingObjectMapper(Func<TSource, TTarget> mappingFunction, IEqualityComparer<TSource> equalityComparer )
        {
            m_MappingFunction = mappingFunction ?? throw new ArgumentNullException(nameof(mappingFunction));
            m_EqualityComparer = equalityComparer ?? throw new ArgumentNullException(nameof(equalityComparer));
            m_Cache = new Dictionary<TSource, TTarget>(m_EqualityComparer);
        }


        public TTarget MapObject(TSource item)
        {
            if (!m_Cache.ContainsKey(item))
            {
                var mappedValue = m_MappingFunction.Invoke(item);
                m_Cache.Add(item, mappedValue);

                return mappedValue;
            }
            else
            {
                return m_Cache[item];                
            }
        }

        /// <summary>
        /// Removes cached values for all objects not present in the specified list from the cache
        /// </summary>
        /// <param name="currentItems"></param>
        public void CleanCache(IEnumerable<TSource> currentItems, bool disposeMappedObjects = true)
        {            
            var itemSet = new HashSet<TSource>(currentItems, m_EqualityComparer);

            var keysToRemove = m_Cache.Keys.Where(key => itemSet.Contains(key) == false).ToList();
            foreach (var key in keysToRemove)
            {
                var value = m_Cache[key];
                if (disposeMappedObjects && value is IDisposable)
                {
                    (value as IDisposable).Dispose();
                }
                m_Cache.Remove(key);
            }            
        }


        public void Dispose()
        {
            CleanCache(Enumerable.Empty<TSource>());
        }
    }
}