using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace C5.Intervals
{
    public class IntervalCollectionFactory<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        public static IIntervalCollection<I, T> CreateCollection(IEnumerable<I> intervals = null, bool allowsOverlaps = true, bool isReadOnly = false, bool allowsReferenceDuplicates = false)
        {
            // The returned collection will have the same 
            Contract.Ensures(Contract.Result<IIntervalCollection<I, T>>().IsReadOnly == isReadOnly);

            // Avoid null values
            if (intervals == null)
                intervals = Enumerable.Empty<I>();

            // Static collection
            if (isReadOnly)
            {
                if (allowsOverlaps)
                    return new LayeredContainmentList<I, T>(intervals);
                else
                    return new StaticFiniteIntervalList<I, T>(intervals);
            }
            // Dynamic collection
            else
            {
                // Allows overlaps
                if (allowsOverlaps)
                    if (allowsReferenceDuplicates)
                        return new DynamicIntervalTree<I, T>(intervals);
                    else
                        return new IntervalBinarySearchTree<I, T>(intervals);
                else
                    return new DoublyLinkedFiniteIntervalTree<I, T>(intervals);
            }
        }
    }
}
