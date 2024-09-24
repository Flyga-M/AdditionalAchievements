using System;
using System.Collections.Generic;

namespace Flyga.AdditionalAchievements
{
    public static class SystemComparisonUtil
    {
        /// <remarks>
        /// Assumes the priority in <paramref name="comparisons"/> to be from
        /// highest to lowest.
        /// </remarks>
        public static int CombineComparisons<T>(T x, T y, IEnumerable<Comparison<T>> comparisons)
        {
            foreach (Comparison<T> comparison in comparisons)
            {
                int result = comparison(x, y);

                if (result != 0)
                {
                    return result;
                }
            }

            return 0;
        }
    }
}
