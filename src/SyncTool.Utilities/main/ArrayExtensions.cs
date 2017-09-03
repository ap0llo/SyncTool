using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.Utilities
{
    public static class ArrayExtensions
    {
        public static IEnumerable<ArraySegment<T>> GetSegments<T>(this T[] array, int maxSegmentSize)
        {
            IEnumerable<ArraySegment<T>> DoGetSegments()
            {
                for (int i = 0; i < array.Length; i += maxSegmentSize)
                {
                    yield return new ArraySegment<T>(array, i, Math.Min(maxSegmentSize, array.Length - i));
                }
            }

            if (maxSegmentSize < 1)
                throw new ArgumentOutOfRangeException(nameof(maxSegmentSize), "Value must not be less than 1");

            if (array.Length == 0)
                return Enumerable.Empty<ArraySegment<T>>();
            
            return DoGetSegments();
        }
    }
}
