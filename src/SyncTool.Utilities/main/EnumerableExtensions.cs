using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Utilities
{
    public static class EnumerableExtensions
    {

        public static string JoinToString(this IEnumerable<object> values, string separator)
        {
            return String.Join(separator, values.ToArray());
        }


        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, T newItem)
        {
            return enumerable.Concat(newItem.Yield());
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable) => new HashSet<T>(enumerable);

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable, IEqualityComparer<T> comparer) =>
            new HashSet<T>(enumerable, comparer);
    }
}
