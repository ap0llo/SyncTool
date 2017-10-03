using System;
using System.Collections.Generic;

namespace SyncTool.Utilities
{
    public class TupleComparer<T1, T2, T3> : IEqualityComparer<(T1, T2, T3)>
    {
        readonly IEqualityComparer<T1> m_FirstItemComparer;
        readonly IEqualityComparer<T2> m_SecondItemComparer;
        readonly IEqualityComparer<T3> m_ThirdItemComparer;

        public TupleComparer(
            IEqualityComparer<T1> firstItemComparer,
            IEqualityComparer<T2> secondItemComparer,
            IEqualityComparer<T3> thirdItemComparer)
        {
            m_FirstItemComparer = firstItemComparer ?? throw new ArgumentNullException(nameof(firstItemComparer));
            m_SecondItemComparer = secondItemComparer ?? throw new ArgumentNullException(nameof(secondItemComparer));
            m_ThirdItemComparer = thirdItemComparer ?? throw new ArgumentNullException(nameof(thirdItemComparer));
        }

        public bool Equals((T1, T2, T3) x, (T1, T2, T3) y) =>
            m_FirstItemComparer.Equals(x.Item1, y.Item1)  &&
            m_SecondItemComparer.Equals(x.Item2, y.Item2) &&
            m_ThirdItemComparer.Equals(x.Item3, y.Item3);

        public int GetHashCode((T1, T2, T3) obj)
        {
            unchecked
            {
                var hash = (m_FirstItemComparer.GetHashCode(obj.Item1) * 397);
                hash ^= m_SecondItemComparer.GetHashCode(obj.Item2);
                hash ^= m_ThirdItemComparer.GetHashCode(obj.Item3);
                return hash;
            }
        }
        
    }
}
