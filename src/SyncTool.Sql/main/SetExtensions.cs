using System.Collections.Generic;

namespace SyncTool.Sql
{
    public static class SetExtensions
    {
        public static void AddAll<T>(this ISet<T> set, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                set.Add(item);
            }
        }
    }
}