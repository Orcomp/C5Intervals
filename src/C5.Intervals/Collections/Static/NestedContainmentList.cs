using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace C5.Intervals
{
    /// <summary>
    /// An in-place implementation of Nested Containment List as described by Aleskeyenko et. al in "Nested
    /// Containment List (NCList): a new algorithm for accelerating interval query of genome alignment and interval databases"
    /// </summary>
    /// <typeparam name="I">The interval type.</typeparam>
    /// <typeparam name="T">The interval endpoint type.</typeparam>
    public class NestedContainmentList<I, T> : CollectionValueBase<I>, IIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Fields

        private readonly Node[] _list;
        private readonly IInterval<T> _span;
        private readonly Sublist _mainSublist;
        private readonly int _count;

        private int _maximumDepth = -1;
        private IInterval<T> _intervalOfMaximumDepth;

        #endregion

        #region Inner Classes

        private struct Sublist
        {
            public Sublist(int start, int length)
                : this()
            {
                Start = start;
                Length = length;
            }

            public int Start { get; private set; }
            public int Length { get; private set; }
            public int End { get { return Start + Length; } }
        }

        private struct Node
        {
            internal I Interval { get; private set; }
            internal Sublist Sublist { get; private set; }

            internal Node(I interval, Sublist sublist)
                : this()
            {
                Interval = interval;
                Sublist = sublist;
            }

            public override string ToString()
            {
                return String.Format("{0} - {1}/{2}", Interval, Sublist.Length, Sublist.Start);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a Nested Containment List with a enumerable of intervals
        /// </summary>
        /// <param name="intervals">A collection of intervals in arbitrary order</param>
        public NestedContainmentList(IEnumerable<I> intervals)
        {
            var intervalsArray = intervals as I[] ?? intervals.ToArray();

            if (intervalsArray.Any())
            {
                _count = intervalsArray.Count();

                Sorting.IntroSort(intervalsArray, 0, _count, IntervalExtensions.CreateComparer<I, T>());

                var totalSection = new Sublist(0, intervalsArray.Count());
                _list = new Node[totalSection.Length];

                // Build nested containment list recursively and save the upper-most list in the class
                _mainSublist = new Sublist(0, createList(intervalsArray, totalSection, totalSection));

                // Save span to allow for constant speeds on later requests
                _span = new IntervalBase<T>(_list[_mainSublist.Start].Interval, _list[_mainSublist.Length + _mainSublist.Start - 1].Interval);
            }
        }

        /// <summary>
        /// A sorted list of IInterval&lt;T&gt; sorted with IntervalComparer&lt;T&gt;
        /// </summary>
        /// <param name="intervals">Sorted intervals</param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns>A list of nodes</returns>
        private int createList(I[] intervals, Sublist source, Sublist target)
        {
            var end = target.End;
            var t = target.Start;

            for (var s = source.Start; s < source.End; s++)
            {
                var interval = intervals[s];
                var contained = 0;
                var length = 0;

                // Continue as long as we have more intervals
                while (s + 1 < source.End)
                {
                    var nextInterval = intervals[s + 1];

                    if (!interval.StrictlyContains(nextInterval))
                        break;

                    contained++;
                    s++;
                }

                if (contained > 0)
                {
                    end -= contained;
                    length = createList(intervals, new Sublist(s - contained + 1, contained), new Sublist(end, contained));
                }

                _list[t++] = new Node(interval, new Sublist(end, length));
            }

            return t - target.Start;
        }

        #endregion

        #region Enumerable

        /// <summary>
        /// Create an enumerator, enumerating the intervals in sorted order - sorted on low endpoint with shortest intervals first
        /// </summary>
        /// <returns>Enumerator</returns>
        public override IEnumerator<I> GetEnumerator() { return Sorted.GetEnumerator(); }

        /// <inheritdoc/>
        public IEnumerable<I> Sorted { get { return getEnumerator(_mainSublist); } }

        private IEnumerable<I> getEnumerator(Sublist sublist)
        {
            // Just for good measures
            if (_list == null || sublist.Length == 0)
                yield break;

            for (var i = sublist.Start; i < sublist.End; i++)
            {
                var node = _list[i];

                // Yield the interval itself before the sublist to maintain sorting order
                yield return node.Interval;

                if (node.Sublist.Length > 0)
                    foreach (var interval in getEnumerator(node.Sublist))
                        yield return interval;
            }
        }

        #endregion

        #region Collection Value

        /// <inheritdoc/>
        public override bool IsEmpty { get { return _count == 0; } }
        /// <inheritdoc/>
        public override int Count { get { return _count; } }
        /// <inheritdoc/>
        public override Speed CountSpeed { get { return Speed.Constant; } }

        /// <inheritdoc/>
        public override I Choose()
        {
            if (IsEmpty)
                throw new NoSuchItemException();

            return _list.First().Interval;
        }

        #endregion

        #region Interval Collection

        #region Properties

        #region Data Structure Properties

        /// <inheritdoc/>
        public bool AllowsOverlaps { get { return true; } }

        /// <inheritdoc/>
        public bool AllowsReferenceDuplicates { get { return true; } }

        #endregion

        #region Collection Properties

        /// <inheritdoc/>
        public IInterval<T> Span { get { return _span; } }

        /// <inheritdoc/>
        public I LowestInterval { get { return _list[0].Interval; } }

        /// <inheritdoc/>
        public IEnumerable<I> LowestIntervals
        {
            get
            {
                if (IsEmpty)
                    yield break;

                var lowestInterval = _list[0].Interval;

                yield return lowestInterval;

                // Iterate through bottom layer as long as the intervals share a low
                for (var i = 1; i < _mainSublist.Length; i++)
                {
                    if (_list[i].Interval.CompareLow(lowestInterval) == 0)
                        yield return _list[i].Interval;
                    else
                        yield break;
                }
            }
        }

        /// <inheritdoc/>
        public I HighestInterval { get { return _list[_mainSublist.Length - 1].Interval; } }

        /// <inheritdoc/>
        public IEnumerable<I> HighestIntervals
        {
            get
            {
                if (IsEmpty)
                    yield break;

                var highestInterval = _list[_mainSublist.Length - 1].Interval;

                yield return highestInterval;

                // Iterate through bottom layer as long as the intervals share a low
                for (var i = _mainSublist.Length - 2; i >= 0; i--)
                {
                    var interval = _list[i].Interval;
                    if (interval.CompareHigh(highestInterval) == 0)
                        yield return interval;
                    else
                        yield break;
                }
            }
        }

        /// <inheritdoc/>
        public int MaximumDepth
        {
            get
            {
                if (_maximumDepth < 0)
                    _maximumDepth = Sorted.MaximumDepth(ref _intervalOfMaximumDepth);

                return _maximumDepth;
            }
        }

        /// <summary>
        /// Get the interval in which the maximum depth is.
        /// </summary>
        public IInterval<T> IntervalOfMaximumDepth
        {
            get
            {
                // If the Maximum Depth is below 0, then the interval of maximum depth has not been set yet
                Contract.Assert(_maximumDepth >= 0 || _intervalOfMaximumDepth == null);

                if (_maximumDepth < 0)
                    _maximumDepth = Sorted.MaximumDepth(ref _intervalOfMaximumDepth);

                return _intervalOfMaximumDepth;
            }
        }

        #endregion

        #endregion

        #region Find Overlaps

        /// <inheritdoc/>
        public IEnumerable<I> FindOverlaps(T query)
        {
            return findOverlaps(_mainSublist, new IntervalBase<T>(query));
        }

        /// <inheritdoc/>
        public IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            return findOverlaps(_mainSublist, query);
        }

        private IEnumerable<I> findOverlaps(Sublist sublist, IInterval<T> query)
        {
            if (IsEmpty || sublist.Length == 0)
                yield break;

            // Find first overlapping interval
            var first = findFirst(sublist, query);

            // If index is out of bound, or interval doesn't overlap, we can just stop our search
            if (first < sublist.Start || sublist.End - 1 < first || !_list[first].Interval.Overlaps(query))
                yield break;

            var last = findLast(sublist, query);

            while (first <= last)
            {
                var node = _list[first++];

                yield return node.Interval;

                if (node.Sublist.Length > 0)
                    // If the interval is contained in the query, all intervals in the sublist must overlap the query
                    foreach (var interval in findOverlaps(node.Sublist, query))
                        yield return interval;
            }
        }

        /// <summary>
        /// Get the index of the first node with an interval the overlaps the query
        /// </summary>
        private int findFirst(Sublist sublist, IInterval<T> query)
        {
            if (query == null || sublist.Length == 0)
                return -1;

            int min = sublist.Start - 1, max = sublist.End;

            while (min + 1 < max)
            {
                var middle = min + (max - min >> 1);

                var interval = _list[middle].Interval;

                var compare = query.Low.CompareTo(interval.High);

                if (compare < 0 || compare == 0 && query.LowIncluded && interval.HighIncluded)
                    max = middle;
                else
                    min = middle;
            }

            // We return min so we know if the query was lower or higher than the list
            return max;
        }

        /// <summary>
        /// Get the index of the last node with an interval the overlaps the query
        /// </summary>
        private int findLast(Sublist sublist, IInterval<T> query)
        {
            if (query == null || sublist.Length == 0)
                return -1;

            int min = sublist.Start - 1, max = sublist.End;

            while (min + 1 < max)
            {
                var middle = min + (max - min >> 1); // Shift one is the same as dividing by 2

                var interval = _list[middle].Interval;

                var compare = interval.Low.CompareTo(query.High);

                if (compare < 0 || compare == 0 && interval.LowIncluded && query.HighIncluded)
                    min = middle;
                else
                    max = middle;
            }

            return min;
        }

        #endregion

        #region Find Overlap

        /// <inheritdoc/>
        public bool FindOverlap(T query, out I overlap)
        {
            return FindOverlap(new IntervalBase<T>(query), out overlap);
        }

        /// <inheritdoc/>
        public bool FindOverlap(IInterval<T> query, out I overlap)
        {
            overlap = null;

            // Check if query overlaps the collection at all
            if (_list == null || !query.Overlaps(Span))
                return false;

            // Find first overlap
            var i = findFirst(_mainSublist, query);

            // Check if index is in bound and if the interval overlaps the query
            var result = _mainSublist.Start <= i && i < _mainSublist.End && _list[i].Interval.Overlaps(query);

            if (result)
                overlap = _list[i].Interval;

            return result;
        }

        #endregion

        #region Count Overlaps

        /// <inheritdoc/>
        public int CountOverlaps(T query)
        {
            return FindOverlaps(query).Count();
        }
        /// <inheritdoc/>
        public int CountOverlaps(IInterval<T> query)
        {
            return FindOverlaps(query).Count();
        }

        #endregion

        #region Gaps

        /// <inheritdoc/>
        public IEnumerable<IInterval<T>> Gaps
        {
            get
            {
                if (IsEmpty)
                    return Enumerable.Empty<IInterval<T>>();

                return _list.Take(_mainSublist.End).Select(x => x.Interval).Gaps();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<IInterval<T>> FindGaps(IInterval<T> query)
        {
            return findOverlapsInMainList(query).Gaps(query);
        }

        private IEnumerable<I> findOverlapsInMainList(IInterval<T> query)
        {
            if (IsEmpty)
                yield break;

            // We know first doesn't overlap so we can increment it before searching
            var first = findFirst(_mainSublist, query);

            // Cache variables to speed up iteration
            var last = _mainSublist.End;
            I interval;

            while (first < last && (interval = _list[first++].Interval).CompareLowHigh(query) <= 0)
                yield return interval;
        }

        #endregion

        #region Extensible

        /// <inheritdoc/>
        public bool IsReadOnly { get { return true; } }

        /// <inheritdoc/>
        public bool Add(I interval)
        {
            throw new ReadOnlyCollectionException();
        }

        /// <inheritdoc/>
        public void AddAll(IEnumerable<I> intervals)
        {
            throw new ReadOnlyCollectionException();
        }

        /// <inheritdoc/>
        public bool Remove(I interval)
        {
            throw new ReadOnlyCollectionException();
        }

        /// <inheritdoc/>
        public void Clear()
        {
            throw new ReadOnlyCollectionException();
        }

        #endregion

        #endregion
    }
}
