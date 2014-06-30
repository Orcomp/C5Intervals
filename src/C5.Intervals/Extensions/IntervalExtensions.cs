using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace C5.Intervals
{
    /// <summary>
    /// Extends IIntervals with convenient methods for overlapping, containment, comparing, equality and hashcode, and string formatting.
    /// </summary>
    public static class IntervalExtensions
    {
        /// <summary>
        /// Check if two intervals overlap.
        /// </summary>
        /// <param name="x">First interval</param>
        /// <param name="y">Second interval</param>
        /// <remarks>True if their intersection is not empty. The meaning should not be confused with <see cref="IntervalRelation.Overlaps"/> and <see cref="IntervalRelation.OverlappedBy"/>!</remarks>
        /// <returns>True if the intervals overlap.</returns>
        [Pure]
        public static bool Overlaps<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Ensures(Contract.Result<bool>() == (x.CompareLowHigh(y) <= 0 && y.CompareLowHigh(x) <= 0));

            // Save compare values to avoid comparing twice in case CompareTo() should be expensive
            int xLowYHighCompare = x.Low.CompareTo(y.High), yLowXHighCompare = y.Low.CompareTo(x.High);
            return (xLowYHighCompare < 0 || xLowYHighCompare == 0 && x.LowIncluded && y.HighIncluded)
                && (yLowXHighCompare < 0 || yLowXHighCompare == 0 && y.LowIncluded && x.HighIncluded);
        }

        /// <summary>
        /// Compare an interval with a point interval to see if they overlap
        /// </summary>
        /// <param name="x">Interval.</param>
        /// <param name="p">Point interval.</param>
        /// <returns>True if the intervals overlap.</returns>
        [Pure]
        public static bool Overlaps<T>(this IInterval<T> x, T p) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(p != null);

            return Overlaps(x, new IntervalBase<T>(p));
        }

        // TODO: Document
        [Pure]
        public static bool OverlapsAny<I, T>(this IInterval<T> interval, IEnumerable<I> intervals)
            where I : IInterval<T>
            where T : IComparable<T>
        {
            return intervals.Any(x => x.Overlaps(interval));
        }

        /// <summary>
        /// Check if one interval contains another interval. The container interval
        /// contains all of the contained interval possibly sharing endpoints.
        /// </summary>
        /// <param name="x">Container interval.</param>
        /// <param name="y">Contained interval.</param>
        /// <returns>True if y is contained in x.</returns>
        [Pure]
        public static bool Contains<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Ensures(Contract.Result<bool>() == (x.CompareLow(y) <= 0 && y.CompareHigh(x) <= 0));

            // Save compare values to avoid comparing twice in case CompareTo() should be expensive
            int lowCompare = x.Low.CompareTo(y.Low), highCompare = y.High.CompareTo(x.High);
            return
                (lowCompare < 0 || lowCompare == 0 && (x.LowIncluded || !y.LowIncluded))
                && (highCompare < 0 || highCompare == 0 && (!y.HighIncluded || x.HighIncluded));
        }

        // TODO: Replace with IsContaining
        /// <summary>
        /// Check if one interval strictly contains another interval. The container interval
        /// contains all of the contained interval without sharing endpoints.
        /// </summary>
        /// <param name="x">Container interval.</param>
        /// <param name="y">Contained interval.</param>
        /// <returns>True if y is strictly contained in x.</returns>
        [Pure]
        public static bool StrictlyContains<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Ensures(Contract.Result<bool>() == (x.CompareLow(y) < 0 && y.CompareHigh(x) < 0));
            Contract.Ensures(Contract.Result<bool>() == x.IsContaining(y));

            // Save compare values to avoid comparing twice in case CompareTo() should be expensive
            int lowCompare = x.Low.CompareTo(y.Low), highCompare = y.High.CompareTo(x.High);
            return
                (lowCompare < 0 || (lowCompare == 0 && x.LowIncluded && !y.LowIncluded))
                && (highCompare < 0 || (highCompare == 0 && !y.HighIncluded && x.HighIncluded));
        }

        /// <summary>
        /// Compare two intervals to determine which one is first in a sorting order.
        /// The interval that starts first and is shortest will come first.
        /// 
        /// Sorting order is determined as follows:
        /// The interval with the lowest low endpoint comes first.
        /// If the low endpoints are equal, the interval with included low endpoint comes first.
        /// If the low endpoint inclusions are equal, the interval with the lowest high endpoint comes first.
        /// If the high endpoints are equal, the interval with excluded high endpoint comes first.
        /// If the high endpoint inclusions are equal, the interval are equal.
        /// </summary>
        /// <param name="x">First interval</param>
        /// <param name="y">Second interval</param>
        /// <returns>Negative if first interval is before the second, 0 if they are equal, otherwise positive.</returns>
        [Pure]
        public static int CompareTo<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Ensures(Contract.Result<int>() == (x.CompareLow(y) != 0 ? x.CompareLow(y) : x.CompareHigh(y)));

            var lowCompare = x.Low.CompareTo(y.Low);

            // Check if x starts y - their lows are the same
            if (lowCompare == 0)
            {
                // If both include or exclude their low endpoint, we don't care which one is first
                if (x.LowIncluded == y.LowIncluded)
                {
                    var highCompare = x.High.CompareTo(y.High);

                    // Check endpoint inclusion, if values are equal, but inclusion is different
                    if (highCompare == 0 && x.HighIncluded != y.HighIncluded)
                        // Excluded high endpoints come before included
                        return !x.HighIncluded ? -1 : 1;

                    return highCompare;
                }

                // x.LowIncluded and y.LowIncluded are different
                // So if x.LowIncluded is true it comes before y
                return x.LowIncluded ? -1 : 1;

            }

            // We can simply compare their low points
            return lowCompare;
        }

        /// <summary>
        /// Compare the low endpoints of two intervals. If the low endpoint values are equal,
        /// an included endpoint precedes an excluded endpoint
        /// </summary>
        /// <param name="x">First interval</param>
        /// <param name="y">Second interval</param>
        /// <returns>Negative if first interval's low is less than second's low endpoint, 0 if they are equal, otherwise positive.</returns>
        [Pure]
        public static int CompareLow<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);

            var compare = x.Low.CompareTo(y.Low);

            // Check endpoint inclusion, if values are equal, but inclusion is different
            if (compare == 0 && x.LowIncluded != y.LowIncluded)
                // Included low endpoints come before excluded
                return x.LowIncluded ? -1 : 1;

            return compare;
        }

        /// <summary>
        /// Compare the low endpoint of an interval to a point.
        /// </summary>
        /// <param name="x">The interval.</param>
        /// <param name="p">The point.</param>
        /// <returns>Negative if the interval's low is less than the point, 0 if they are equal, otherwise positive.</returns>
        [Pure]
        public static int CompareLow<T>(this IInterval<T> x, T p) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(p != null);
            Contract.Ensures(Contract.Result<int>() == x.CompareLow(new IntervalBase<T>(p)));

            var compare = x.Low.CompareTo(p);

            // Check endpoint inclusion, if values are equal
            if (compare == 0)
                return x.LowIncluded ? 0 : 1;

            return compare;
        }

        /// <summary>
        /// Compare the high endpoints of two intervals. If the high endpoint values are equal,
        /// an excluded endpoint precedes an included endpoint
        /// </summary>
        /// <param name="x">First interval</param>
        /// <param name="y">Second interval</param>
        /// <returns>Negative if first interval's high is less than second's high endpoint, 0 if they are equal, otherwise positive.</returns>
        [Pure]
        public static int CompareHigh<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);

            var compare = x.High.CompareTo(y.High);

            // Check endpoint inclusion, if values are equal, but inclusion is different
            if (compare == 0 && x.HighIncluded != y.HighIncluded)
                // Excluded high endpoints come before included
                return !x.HighIncluded ? -1 : 1;

            return compare;
        }

        /// <summary>
        /// Compare the high endpoint of an interval to a point.
        /// </summary>
        /// <param name="x">The interval.</param>
        /// <param name="p">The point.</param>
        /// <returns>Negative if the interval's high is less than the point, 0 if they are equal, otherwise positive.</returns>
        [Pure]
        public static int CompareHigh<T>(this IInterval<T> x, T p) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(p != null);
            Contract.Ensures(Contract.Result<int>() == x.CompareHigh(new IntervalBase<T>(p)));

            var compare = x.High.CompareTo(p);

            // Check endpoint inclusion, if values are equal, but inclusion is different
            if (compare == 0)
                return !x.HighIncluded ? -1 : 0;

            return compare;
        }

        /// <summary>
        /// Compare the low endpoint of first interval to the high endpoint of the second interval.
        /// </summary>
        /// <param name="x">First interval.</param>
        /// <param name="y">Second interval.</param>
        /// <returns>Negative if first interval's low is less than second's high endpoint, 0 if they are equal, otherwise positive.</returns>
        [Pure]
        public static int CompareLowHigh<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);

            var compare = x.Low.CompareTo(y.High);

            // Check endpoint inclusion, if values are equal, but inclusion is different
            if (compare == 0)
                // Excluded high endpoints come before included
                return x.LowIncluded && y.HighIncluded ? 0 : 1;

            return compare;
        }
        /// <summary>
        /// Compare the high endpoint of first interval to the low endpoint of the second interval.
        /// </summary>
        /// <param name="x">First interval.</param>
        /// <param name="y">Second interval.</param>
        /// <returns>Negative if first interval's high is less than second's low endpoint, 0 if they are equal, otherwise positive.</returns>
        [Pure]
        public static int CompareHighLow<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);

            var compare = x.High.CompareTo(y.Low);

            // Check endpoint inclusion, if values are equal, but inclusion is different
            if (compare == 0)
                // Excluded high endpoints come before included
                return x.HighIncluded && y.LowIncluded ? 0 : -1;

            return compare;
        }

        /// <summary>
        /// Compare the endpoint values of an interval with each other.
        /// </summary>
        /// <param name="x">The interval.</param>
        /// <returns>Negative if the high endpoint value is less than the low endpoint value, 0 if they are equal, otherwise positive.</returns>
        [Pure]
        public static int CompareEndpointsValues<T>(this IInterval<T> x) where T : IComparable<T>
        {
            Contract.Requires(x != null);

            return x.Low.CompareTo(x.High);
        }

        /// <summary>
        /// Check if two intervals are equal, i.e. have the same low and high endpoint including endpoint inclusion.
        /// </summary>
        /// <param name="x">First interval.</param>
        /// <param name="y">Second interval.</param>
        /// <returns>True if both endpoints are equal.</returns>
        [Pure]
        public static bool IntervalEquals<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Ensures(Contract.Result<bool>() == (CompareTo(x, y) == 0));

            return x.Low.CompareTo(y.Low) == 0 && x.High.CompareTo(y.High) == 0 && x.LowIncluded == y.LowIncluded &&
                   x.HighIncluded == y.HighIncluded;
        }

        /// <summary>
        /// Get the interval in which two intervals overlap.
        /// </summary>
        /// <param name="x">First interval.</param>
        /// <param name="y">Second interval.</param>
        /// <typeparam name="T">The endpoint type.</typeparam>
        /// <returns>An interval of intersection.</returns>
        /// <exception cref="ArgumentException">If the intervals do not overlap.</exception>
        [Pure]
        public static IInterval<T> Overlap<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Requires(x.Overlaps(y));
            // The intersection will be contained in both intervals
            Contract.Ensures(x.Contains(Contract.Result<IInterval<T>>()));
            Contract.Ensures(y.Contains(Contract.Result<IInterval<T>>()));

            var low = x.CompareLow(y) > 0 ? x : y;
            var high = x.CompareHigh(y) < 0 ? x : y;

            return new IntervalBase<T>(low, high);
        }

        /*public static IInterval<T> Subtract<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            // TODO
            return null;
        }*/

        public static IInterval<T> Gap<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Requires(x.IsBefore(y));

            return new IntervalBase<T>(x.High, y.Low, !x.HighIncluded, !y.LowIncluded);
        }

        /// <summary>
        /// Creates an interval from the lowest low and the highest high of the two intervals. The joined span contains both intervals.
        /// </summary>
        /// <param name="x">The first interval.</param>
        /// <param name="y">The second interval.</param>
        /// <typeparam name="T">The endpoint type.</typeparam>
        /// <returns>The joined span.</returns>
        public static IInterval<T> JoinedSpan<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            // The intersection will be contained in both intervals
            Contract.Ensures(Contract.Result<IInterval<T>>().Contains(x));
            Contract.Ensures(Contract.Result<IInterval<T>>().Contains(y));
            // The endpoints of the joined span is each equal to one of the endpoints of the two intervals
            Contract.Ensures(x.CompareLow(Contract.Result<IInterval<T>>()) == 0 || y.CompareLow(Contract.Result<IInterval<T>>()) == 0);
            Contract.Ensures(x.CompareHigh(Contract.Result<IInterval<T>>()) == 0 || y.CompareHigh(Contract.Result<IInterval<T>>()) == 0);

            return new IntervalBase<T>(x.LowestLow(y), x.HighestHigh(y));
        }

        /// <summary>
        /// Returns the interval with the highest high endpoint. If equal the first interval is favored.
        /// </summary>
        /// <param name="x">First interval.</param>
        /// <param name="y">Second interval.</param>
        /// <typeparam name="T">Endpoint value type</typeparam>
        /// <returns>The interval with the highest high endpoint.</returns>
        [Pure]
        public static IInterval<T> HighestHigh<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            return x.CompareHigh(y) >= 0 ? x : y;
        }

        /// <summary>
        /// Returns the interval with the lowest low endpoint. If equal the first interval is favored.
        /// </summary>
        /// <param name="x">First interval.</param>
        /// <param name="y">Second interval.</param>
        /// <typeparam name="T">Endpoint value type</typeparam>
        /// <returns>The interval with the lowest low endpoint.</returns>
        [Pure]
        public static IInterval<T> LowestLow<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            return x.CompareLow(y) <= 0 ? x : y;
        }

        /// <summary>
        /// Check if an interval is valid.
        /// </summary>
        /// <param name="x">The interval.</param>
        /// <typeparam name="T">The endpoint type.</typeparam>
        /// <returns>True if the interval is valid.</returns>
        [Pure]
        public static bool IsValidInterval<T>(this IInterval<T> x) where T : IComparable<T>
        {
            if (x == null || x.Low == null || x.High == null)
                return false;

            var compare = x.Low.CompareTo(x.High);
            return compare < 0 || compare == 0 && x.LowIncluded && x.HighIncluded;
        }

        /// <summary>
        /// Check if an interval is a point.
        /// </summary>
        /// <param name="x">The interval.</param>
        /// <typeparam name="T">The endpoint type.</typeparam>
        /// <returns>True if Low and High are equal and both endpoints are included, false otherwise.</returns>
        [Pure]
        public static bool IsPoint<T>(this IInterval<T> x) where T : IComparable<T>
        {
            Contract.Requires(x != null);

            return x.Low.CompareTo(x.High) == 0 && x.LowIncluded && x.HighIncluded;
        }

        /// <summary>
        /// Get the hashcode for an interval based on endpoints.
        /// </summary>
        /// <param name="x">The interval.</param>
        /// <typeparam name="T">The endpoint type.</typeparam>
        /// <returns>The hashcode based on the interval.</returns>
        [Pure]
        public static int GetIntervalHashCode<T>(this IInterval<T> x) where T : IComparable<T>
        {
            Contract.Requires(x != null);

            unchecked
            {
                return (((527
                    + x.Low.GetHashCode()) * 31
                    + x.LowIncluded.GetHashCode()) * 31
                    + x.High.GetHashCode()) * 31
                    + x.HighIncluded.GetHashCode();
            }
        }

        /// <summary>
        /// Create a string representing the interval.
        /// </summary>
        /// <param name="x">The interval.</param>
        /// <param name="delimiter">The delimiter separating the low and high endpoint value.</param>
        /// <remarks>Closed intervals are represented with square brackets [a:b] and open with round brackets (a:b).</remarks>
        /// <returns>The string representation.</returns>
        [Pure]
        public static string ToIntervalString<T>(this IInterval<T> x, string delimiter = ":") where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Ensures(Contract.Result<string>().Length > 0);

            return String.Format("{0}{1}{2}{3}{4}",
                x.LowIncluded ? "[" : "(",
                x.Low,
                delimiter,
                x.High,
                x.HighIncluded ? "]" : ")");
        }

        public static IComparer<I> CreateComparer<I, T>()
            where I : IInterval<T>
            where T : IComparable<T>
        {
            return ComparerFactory<I>.CreateComparer((x, y) => x.CompareTo(y));
        }

        public static IComparer<I> CreateLowComparer<I, T>()
            where I : IInterval<T>
            where T : IComparable<T>
        {
            return ComparerFactory<I>.CreateComparer((x, y) => x.CompareLow(y));
        }

        public static IComparer<I> CreateHighComparer<I, T>()
            where I : IInterval<T>
            where T : IComparable<T>
        {
            return ComparerFactory<I>.CreateComparer((x, y) => x.CompareHigh(y));
        }

        public static IComparer<I> CreateReversedComparer<I, T>()
            where I : IInterval<T>
            where T : IComparable<T>
        {
            return ComparerFactory<I>.CreateComparer((x, y) => { var compare = y.CompareHigh(x); return compare != 0 ? compare : y.CompareLow(x); });
        }

        /// <summary>
        /// Create an enumerable, enumerating all unique endpoints in intervals.
        /// </summary>
        /// <param name="intervals">The intervals.</param>
        /// <typeparam name="T">The endpoint type.</typeparam>
        /// <returns>Unique endpoints from intervals.</returns>
        [Pure]
        public static IEnumerable<T> UniqueEndpointValues<T>(this IEnumerable<IInterval<T>> intervals)
            where T : IComparable<T>
        {
            var array = intervals as IInterval<T>[] ?? intervals.ToArray();
            var intervalCount = array.Count();

            // Save all endpoints to array
            var endpoints = new T[intervalCount * 2];
            for (var i = 0; i < intervalCount; i++)
            {
                var interval = array[i];

                endpoints[i * 2] = interval.Low;
                endpoints[i * 2 + 1] = interval.High;
            }

            // Sort endpoints
            Sorting.IntroSort(endpoints);

            // Remove duplicate endpoints
            var uniqueEndpoints = new T[intervalCount * 2];
            var endpointCount = 0;

            foreach (var endpoint in endpoints)
                if (endpointCount == 0 || uniqueEndpoints[endpointCount - 1].CompareTo(endpoint) < 0)
                    uniqueEndpoints[endpointCount++] = endpoint;

            return uniqueEndpoints.Take(endpointCount);
        }

        /// <summary>
        /// Get the span of a collection of intervals. The span is the smallest interval that spans
        /// all intervals in the collection. The interval's low is the lowest low endpoint in the
        /// collection and the high is the highest high endpoint.
        /// </summary>
        /// <param name="intervals">The collection of intervals.</param>
        /// <typeparam name="T">The endpoint type.</typeparam>
        /// <returns>The span of the intervals.</returns>
        /// <exception cref="InvalidOperationException">If the collection is empty.</exception>
        [Pure]
        public static IInterval<T> Span<T>(this IEnumerable<IInterval<T>> intervals)
            where T : IComparable<T>
        {
            Contract.Requires(intervals.Any());

            var enumerator = intervals.GetEnumerator();

            enumerator.MoveNext();
            var low = enumerator.Current;
            var high = enumerator.Current;

            while (enumerator.MoveNext())
            {
                var interval = enumerator.Current;

                if (interval.CompareLow(low) < 0)
                    low = interval;
                if (high.CompareHigh(interval) < 0)
                    high = interval;
            }

            return new IntervalBase<T>(low, high);
        }

        [Pure]
        public static I LowestInterval<I, T>(this IEnumerable<I> intervals)
            where I : IInterval<T>
            where T : IComparable<T>
        {
            Contract.Requires(intervals.Any());

            var enumerator = intervals.GetEnumerator();
            enumerator.MoveNext();
            var low = enumerator.Current;

            while (enumerator.MoveNext())
                if (enumerator.Current.CompareLow(low) < 0)
                    low = enumerator.Current;

            return low;
        }

        [Pure]
        public static IEnumerable<I> LowestIntervals<I, T>(this IEnumerable<I> intervals)
            where I : IInterval<T>
            where T : IComparable<T>
        {
            Contract.Requires(intervals.Any());

            var enumerator = intervals.GetEnumerator();
            if (!enumerator.MoveNext())
                return Enumerable.Empty<I>();

            var list = new LinkedList<I>();
            var low = enumerator.Current;
            list.Add(low);

            while (enumerator.MoveNext())
            {
                var compare = enumerator.Current.CompareLow(low);

                if (compare > 0)
                    continue;

                if (compare < 0)
                    list.Clear();

                list.Add(low = enumerator.Current);
            }

            return list;
        }

        [Pure]
        public static IInterval<T> HighestInterval<T>(this IEnumerable<IInterval<T>> intervals)
            where T : IComparable<T>
        {
            Contract.Requires(intervals.Any());

            var enumerator = intervals.GetEnumerator();
            enumerator.MoveNext();
            var highestInterval = enumerator.Current;

            while (enumerator.MoveNext())
                if (highestInterval.CompareHigh(enumerator.Current) < 0)
                    highestInterval = enumerator.Current;

            return highestInterval;
        }

        [Pure]
        public static IEnumerable<I> HighestIntervals<I, T>(this IEnumerable<I> intervals)
            where I : IInterval<T>
            where T : IComparable<T>
        {
            Contract.Requires(intervals.Any());

            var enumerator = intervals.GetEnumerator();
            if (!enumerator.MoveNext())
                return Enumerable.Empty<I>();

            var list = new LinkedList<I>();
            var high = enumerator.Current;
            list.Add(high);

            while (enumerator.MoveNext())
            {
                var compare = high.CompareHigh(enumerator.Current);

                if (compare > 0)
                    continue;

                if (compare < 0)
                    list.Clear();

                list.Add(high = enumerator.Current);
            }

            return list;
        }

        /// <summary>
        /// The maximum number of intervals overlapping at a single point in the collection.
        /// </summary>
        /// <remarks>The point of maximum depth may not be representable with an endpoint value, as it could be between two descrete values.</remarks>
        /// <param name="intervals">The collection of intervals.</param>
        /// <param name="intervalOfMaximumDepth">The interval in which the maximum depth is.</param>
        /// <param name="isSorted">True if the intervals are sorted based on low then high endpoint. If false the collection will be sorted.</param>
        /// <typeparam name="I">The interval type.</typeparam>
        /// <typeparam name="T">The endpoint type.</typeparam>
        /// <returns>The maximum depth.</returns>
        [Pure]
        public static int MaximumDepth<I, T>(this IEnumerable<I> intervals, ref IInterval<T> intervalOfMaximumDepth, bool isSorted = true)
            where I : IInterval<T>
            where T : IComparable<T>
        {
            Contract.Requires(intervals != null);
            Contract.Requires(IntervalContractHelper.IsSorted<I, T>(intervals, isSorted));
            Contract.Ensures(Contract.Result<int>() >= 0);
            Contract.Ensures(Contract.Result<int>() == 0 || IntervalCollectionContractHelper.CountOverlaps(((IEnumerable<IInterval<T>>) intervals), Contract.ValueAtReturn(out intervalOfMaximumDepth)) == Contract.Result<int>());

            // Sort the intervals if necessary
            if (!isSorted)
            {
                var sortedIntervals = intervals as I[] ?? intervals.ToArray();
                Sorting.IntroSort(sortedIntervals, 0, sortedIntervals.Length, CreateLowComparer<I, T>());
                intervals = sortedIntervals;
            }

            var max = 0;

            // Create queue sorted on high intervals
            var comparer = ComparerFactory<IInterval<T>>.CreateComparer(CompareHigh);
            var queue = new IntervalHeap<IInterval<T>>(comparer);

            // Loop through intervals in sorted order
            foreach (var interval in intervals)
            {
                // Remove all intervals from the queue not overlapping the current interval
                while (!queue.IsEmpty && interval.CompareLowHigh(queue.FindMin()) > 0)
                    queue.DeleteMin();

                queue.Add(interval);

                if (queue.Count > max)
                {
                    max = queue.Count;
                    // Create a new interval when new maximum is found.
                    // The low is the current intervals low due to the intervals being sorted.
                    // The high is the smallest high in the queue.
                    intervalOfMaximumDepth = new IntervalBase<T>(interval, queue.FindMin());
                }
            }

            return max;
        }

        public static bool IsContiguous<T>(this IEnumerable<IInterval<T>> intervals) where T : IComparable<T>
        {
            return intervals.ForAllConsecutiveElements((x, y) => x.IsMeeting(y));
        }

        [Pure]
        public static IEnumerable<IInterval<T>> Gaps<T>(this IEnumerable<IInterval<T>> intervals, IInterval<T> span = null, bool isSorted = true)
            where T : IComparable<T>
        {
            Contract.Requires(intervals != null);
            // Intervals must be sorted
            Contract.Requires(IntervalContractHelper.IsSorted<IInterval<T>, T>(intervals, isSorted));

            // The gaps don't overlap the collection and they are within the span
            Contract.Ensures(span == null || Contract.ForAll(Contract.Result<IEnumerable<IInterval<T>>>(), span.Contains));
            Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IInterval<T>>>(), x => !x.OverlapsAny(intervals)));
            // Each gap will be met by an interval in the collection
            Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IInterval<T>>>(), x => span != null && x.CompareLow(span) == 0 || intervals.Any(y => x.Low.CompareTo(y.High) == 0 && x.LowIncluded != y.HighIncluded)));
            // Each gap will be meet an interval in the collection
            Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IInterval<T>>>(), x => span != null && x.CompareHigh(span) == 0 || intervals.Any(y => x.High.CompareTo(y.Low) == 0 && x.HighIncluded != y.LowIncluded)));

            // Sort the intervals if necessary
            if (!isSorted)
            {
                var sortedIntervals = intervals as IInterval<T>[] ?? intervals.ToArray();
                Sorting.IntroSort(sortedIntervals, 0, sortedIntervals.Length, CreateLowComparer<IInterval<T>, T>());
                intervals = sortedIntervals;
            }

            Contract.Assert(intervals.IsSorted<IInterval<T>, T>());

            var useSpan = span != null;
            var enumerator = intervals.GetEnumerator();

            // Check if empty
            if (!enumerator.MoveNext())
            {
                if (useSpan)
                    // Return span as whole span will be a gap
                    yield return new IntervalBase<T>(span);

                // Break as no more gaps can exist
                yield break;
            }

            // Get the first interval
            var highestHigh = enumerator.Current;

            // Check for a gap within the span before the lowest endpoint
            if (useSpan && span.CompareLow(highestHigh) < 0)
                yield return new IntervalBase<T>(span.Low, highestHigh.Low, span.LowIncluded, !highestHigh.LowIncluded);

            // Iterate through the sorted intervals
            while (enumerator.MoveNext())
            {
                var next = enumerator.Current;

                // Check for gap and create interval if there is one
                if (highestHigh.IsBefore(next))
                    yield return Gap(highestHigh, next);

                // Update highest high if next is higher
                if (next.CompareHigh(highestHigh) > 0)
                    highestHigh = next;
            }

            // Check for a gap within the span after the highest endpoint
            if (useSpan && highestHigh.CompareHigh(span) < 0)
                yield return new IntervalBase<T>(highestHigh.High, span.High, !highestHigh.HighIncluded, span.HighIncluded);
        }


        public static void ActionOnOverlaps<I, T>(this IEnumerable<I> intervals, Action<IEnumerable<I>> action, bool isSorted = true)
            where I : IInterval<T>
            where T : IComparable<T>
        {
            Contract.Requires(intervals != null);
            // Intervals must be sorted
            Contract.Requires(IntervalContractHelper.IsSorted<I, T>(intervals, isSorted));

            // Sort the intervals if necessary
            if (!isSorted)
            {
                var sortedIntervals = intervals as I[] ?? intervals.ToArray();
                Sorting.IntroSort(sortedIntervals, 0, sortedIntervals.Length, CreateLowComparer<I, T>());
                intervals = sortedIntervals;
            }

            // Create queue sorted on high intervals
            var comparer = ComparerFactory<I>.CreateComparer((x, y) => x.CompareHigh(y));
            var queue = new IntervalHeap<I>(comparer);

            // Loop through intervals in sorted order
            foreach (var interval in intervals)
            {
                var callAction = true;

                // Remove all intervals from the queue not overlapping the current interval
                while (!queue.IsEmpty && interval.CompareLowHigh(queue.FindMin()) > 0)
                {
                    if (callAction)
                    {
                        action(queue);
                        callAction = false;
                    }
                    queue.DeleteMin();
                }

                queue.Add(interval);

                Contract.Assert(Contract.ForAll(queue, x => Contract.ForAll(queue, y => x.Overlaps(y))));
            }

            // Call the action on the remaining overlapping intervals in the queue
            action(queue);
        }


        public static IEnumerable<KeyValuePair<IInterval<T>, IEnumerable<I>>> Collapse<I, T>(this IEnumerable<I> intervals, bool isSorted = true)
            where I : IInterval<T>
            where T : IComparable<T>
        {
            Contract.Requires(intervals != null);
            // Intervals must be sorted
            Contract.Requires(IntervalContractHelper.IsSorted<I, T>(intervals, isSorted));

            Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<KeyValuePair<IInterval<T>, IEnumerable<I>>>>(), pair => Contract.ForAll(pair.Value, x => x.Contains(pair.Key))));
            Contract.Ensures(Contract.Result<IEnumerable<KeyValuePair<IInterval<T>, IEnumerable<I>>>>().Select(x => x.Key).IsSorted<IInterval<T>, T>());
            Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<KeyValuePair<IInterval<T>, IEnumerable<I>>>>(), pair => IntervalCollectionContractHelper.CollectionEquals(pair.Value, intervals.Where(x => x.Contains(pair.Key)))));
            Contract.Ensures(IntervalCollectionContractHelper.CollectionIntervalEquals<IInterval<T>, T>(Contract.Result<IEnumerable<KeyValuePair<IInterval<T>, IEnumerable<I>>>>().Select(pair => pair.Key).Gaps(), intervals.Cast<IInterval<T>>().Gaps(isSorted: isSorted)));


            // Sort the intervals if necessary
            if (!isSorted)
            {
                var sortedIntervals = intervals as I[] ?? intervals.ToArray();
                Sorting.IntroSort(sortedIntervals, 0, sortedIntervals.Length, CreateLowComparer<I, T>());
                intervals = sortedIntervals;
            }

            // Create queue sorted on high intervals
            var queue = new IntervalHeap<I>(CreateHighComparer<I, T>());

            var enumerator = intervals.GetEnumerator();

            if (!enumerator.MoveNext())
                yield break;

            var previous = enumerator.Current;
            queue.Add(previous);

            // Loop through intervals in sorted order
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;

                // Remove all intervals from the queue not overlapping the current interval
                while (!queue.IsEmpty && queue.FindMin().CompareHighLow(current) < 0)
                    foreach (var pair in collapse<I, T>(queue, previous))
                        yield return pair;

                // Get the interval from the last interval's low up to current
                if (!queue.IsEmpty && previous.CompareLow(current) < 0)
                {
                    yield return new KeyValuePair<IInterval<T>, IEnumerable<I>>(
                        new IntervalBase<T>(previous.Low, current.Low, previous.LowIncluded, !current.LowIncluded),
                        queue.ToArray()
                    );
                }

                queue.Add(current);

                previous = current;
            }

            foreach (var pair in collapse<I, T>(queue, previous))
                yield return pair;
        }

        private static IEnumerable<KeyValuePair<IInterval<T>, IEnumerable<I>>> collapse<I, T>(IPriorityQueue<I> queue, I previous)
            where I : IInterval<T>
            where T : IComparable<T>
        {
            if (queue.IsEmpty)
                yield break;

            var current = queue.FindMin();

            if (previous.CompareLowHigh(current) < 0)
            {
                yield return new KeyValuePair<IInterval<T>, IEnumerable<I>>(
                    new IntervalBase<T>(previous, current),
                    queue.ToArray()
                );
            }

            queue.DeleteMin();
            previous = current;

            // Empty queue
            while (!queue.IsEmpty)
            {
                current = queue.FindMin();

                if (previous.CompareHigh(current) < 0)
                {
                    yield return new KeyValuePair<IInterval<T>, IEnumerable<I>>(
                        new IntervalBase<T>(previous.High, current.High, !previous.HighIncluded, current.HighIncluded),
                        queue.ToArray()
                        );
                }

                queue.DeleteMin();
                previous = current;
            }
        }
    }

    /// <summary>
    /// Convenient extensions.
    /// </summary>
    // TODO: Find the proper file for the extension class
    public static class C5Extensions
    {

        /// <summary>
        /// Convert an IEnumerator to an IEnumerable.
        /// </summary>
        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext()) yield return enumerator.Current;
        }

        [Pure]
        public static bool IsSorted<I, T>(this IEnumerable<I> collection)
            where I : IInterval<T>
            where T : IComparable<T>
        {
            return collection.IsSorted(IntervalExtensions.CreateComparer<I, T>());
        }

        /// <summary>
        /// Check if an IEnumerable is sorted in non-descending order.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <typeparam name="T">The element type.</typeparam>
        /// <returns>True if sorted.</returns>
        [Pure]
        public static bool IsSorted<T>(this IEnumerable<T> collection) where T : IComparable<T>
        {
            Contract.Requires(collection != null);

            using (var enumerator = collection.GetEnumerator())
            {

                if (enumerator.MoveNext())
                {
                    var previous = enumerator.Current;

                    while (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;

                        if (previous.CompareTo(current) > 0)
                            return false;

                        previous = current;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Check if an IEnumerable is sorted in non-descending order using a comparer.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="comparer">The comparer for the collection.</param>
        /// <typeparam name="T">The element type.</typeparam>
        /// <returns>True if sorted.</returns>
        [Pure]
        public static bool IsSorted<T>(this IEnumerable<T> collection, IComparer<T> comparer)
        {
            Contract.Requires(collection != null);
            Contract.Requires(comparer != null);

            var enumerator = collection.GetEnumerator();

            if (enumerator.MoveNext())
            {
                var previous = enumerator.Current;

                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;

                    if (comparer.Compare(previous, current) > 0)
                        return false;

                    previous = current;
                }
            }

            return true;
        }

        [Pure]
        public static bool ForAllConsecutiveElements<T>(this IEnumerable<T> collection, Func<T, T, bool> predicate)
        {
            Contract.Requires(collection != null);
            Contract.Requires(predicate != null);

            var enumerator = collection.GetEnumerator();

            if (enumerator.MoveNext())
            {
                var previous = enumerator.Current;

                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;

                    if (!predicate(previous, current))
                        return false;

                    previous = current;
                }
            }

            return true;
        }
    }
}