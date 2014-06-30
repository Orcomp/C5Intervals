using System;
using System.Collections.Generic;

namespace C5.Intervals
{
    public class IntervalContractHelper
    {
        /// <summary>
        /// Checks that the collection is sorted. Will only run in debug!
        /// </summary>
        public static bool IsSorted<I, T>(IEnumerable<I> collection, bool shouldBeSorted)
            where I : IInterval<T>
            where T : IComparable<T>
        {
#if DEBUG
            return !shouldBeSorted || collection.IsSorted(IntervalExtensions.CreateComparer<I, T>());
#else
            return true;
#endif
        }
    }
}
