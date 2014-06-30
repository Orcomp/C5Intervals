using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace C5.Intervals
{
    /// <summary>
    /// An implementation of Nested Containment List as described by Aleskeyenko et. al in "Nested
    /// Containment List (NCList): a new algorithm for accelerating interval query of genome
    /// alignment and interval databases"
    /// </summary>
    /// <typeparam name="I">The interval type.</typeparam>
    /// <typeparam name="T">The interval endpoint type.</typeparam>
    public class NestedContainmentList2<I, T> : CollectionValueBase<I>, IIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        private readonly Node[] _list;
        private readonly IInterval<T> _span;
        private readonly int _count;
        private int _maximumDepth;
        private IInterval<T> _intervalOfMaximumDepth;

        #region Node nested classes

        struct Node
        {
            internal I Interval { get; private set; }
            internal Node[] Sublist { get; private set; }

            internal Node(I interval, Node[] sublist)
                : this()
            {
                Interval = interval;
                Sublist = sublist;
            }

            public override string ToString()
            {
                return Interval.ToString();
            }
        }

        #endregion

        #region constructors

        /// <summary>
        /// A sorted list of IInterval&lt;T&gt; sorted with IntervalComparer&lt;T&gt;
        /// </summary>
        /// <param name="intervals">Sorted intervals</param>
        /// <returns>A list of nodes</returns>
        private static Node[] createList(IEnumerable<I> intervals)
        {
            // List to hold the nodes
            IList<Node> list = new ArrayList<Node>();

            // Iterate through the intervals to build the list
            using (var enumerator = intervals.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    // Remember the previous node so we can check if the next nodes are contained in it
                    var previous = enumerator.Current;
                    var sublist = new ArrayList<I>();

                    // Loop through intervals
                    while (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;

                        if (previous.StrictlyContains(current))
                            // Add contained intervals to sublist for previous node
                            sublist.Add(current);
                        else
                        {
                            // The current interval wasn't contained in the prevoius node, so we can finally add the previous to the list
                            list.Add(new Node(previous, (sublist.IsEmpty ? null : createList(sublist))));

                            // Reset the looped values
                            if (!sublist.IsEmpty)
                                sublist = new ArrayList<I>();

                            previous = current;
                        }
                    }

                    // Add the last node to the list when we are done looping through them
                    list.Add(new Node(previous, createList(sublist.ToArray())));
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// Create a Nested Containment List with a enumerable of intervals
        /// </summary>
        /// <param name="intervals">A collection of intervals in arbitrary order</param>
        public NestedContainmentList2(IEnumerable<I> intervals)
        {
            var intervalsArray = intervals as I[] ?? intervals.ToArray();

            if (!intervalsArray.Any())
                return;

            _count = intervalsArray.Count();

            Sorting.IntroSort(intervalsArray, 0, _count, IntervalExtensions.CreateComparer<I, T>());

            // Build nested containment list recursively and save the upper-most list in the class
            _list = createList(intervalsArray);

            // Save span to allow for constant speeds on later requests
            IInterval<T> i = _list[0].Interval, j = _list[_list.Length - 1].Interval;
            _span = new IntervalBase<T>(i, j);
        }

        #endregion

        #region Collection Value

        /// <inheritdoc/>
        public override bool IsEmpty { get { return Count == 0; } }
        /// <inheritdoc/>
        public override int Count { get { return _count; } }
        /// <inheritdoc/>
        public override Speed CountSpeed { get { return Speed.Constant; } }

        /// <inheritdoc/>
        public override I Choose()
        {
            if (IsEmpty)
                throw new NoSuchItemException();

            return _list[0].Interval;
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

        public I LowestInterval { get { return _list[0].Interval; } }

        public IEnumerable<I> LowestIntervals
        {
            get
            {
                if (IsEmpty)
                    yield break;

                var lowestInterval = _list[0].Interval;

                yield return lowestInterval;

                // Iterate through bottom layer as long as the intervals share a low
                for (var i = 1; i < _list.Length; i++)
                {
                    if (_list[i].Interval.CompareLow(lowestInterval) == 0)
                        yield return _list[i].Interval;
                    else
                        yield break;
                }
            }
        }

        public I HighestInterval { get { return _list[_list.Length - 1].Interval; } }
        public IEnumerable<I> HighestIntervals
        {
            get
            {
                if (IsEmpty)
                    yield break;

                var highestInterval = _list[_list.Length - 1].Interval;

                yield return highestInterval;

                // Iterate through bottom layer as long as the intervals share a low
                for (var i = _list.Length - 2; i >= 0; i--)
                {
                    if (_list[i].Interval.CompareHigh(highestInterval) == 0)
                        yield return _list[i].Interval;
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
                if (_maximumDepth < 0)
                    _maximumDepth = Sorted.MaximumDepth(ref _intervalOfMaximumDepth);

                return _intervalOfMaximumDepth;
            }
        }

        #endregion

        #endregion

        #region Enumerable

        /// <summary>
        /// Create an enumerator, enumerating the intervals in sorted order - sorted on low endpoint with shortest intervals first
        /// </summary>
        /// <returns>Enumerator</returns>
        public override IEnumerator<I> GetEnumerator() { return Sorted.GetEnumerator(); }

        /// <inheritdoc/>
        public IEnumerable<I> Sorted { get { return getEnumerator(_list); } }

        private IEnumerable<I> getEnumerator(IEnumerable<Node> list)
        {
            // Just for good measures
            if (list == null)
                yield break;

            foreach (var node in list)
            {
                // Yield the interval itself before the sublist to maintain sorting order
                yield return node.Interval;

                if (node.Sublist != null)
                    foreach (var interval in getEnumerator(node.Sublist))
                        yield return interval;
            }
        }

        #endregion

        /// <inheritdoc/>
        public IEnumerable<I> FindOverlaps(T query)
        {
            if (ReferenceEquals(query, null))
                throw new NullReferenceException("Query can't be null");

            return findOverlaps(_list, new IntervalBase<T>(query));
        }

        // TODO: Test speed difference between version that takes overlap-loop and upper and low bound loop
        private static IEnumerable<I> findOverlaps(Node[] list, IInterval<T> query)
        {
            if (list == null)
                yield break;

            int first, last;

            // Find first overlapping interval
            first = findFirst(list, query);

            // If index is out of bound, or interval doesn't overlap, we can just stop our search
            if (first < 0 || list.Length - 1 < first || !list[first].Interval.Overlaps(query))
                yield break;

            last = findLast(list, query);

            while (first <= last)
            {
                var node = list[first++];

                yield return node.Interval;

                if (node.Sublist != null)
                    foreach (var interval in findOverlaps(node.Sublist, query))
                        yield return interval;
            }
        }

        /// <summary>
        /// Get the index of the first node with an interval the overlaps the query
        /// </summary>
        private static int findFirst(Node[] list, IInterval<T> query)
        {
            if (query == null || list.Length == 0)
                return -1;

            int min = -1, max = list.Length;

            while (max - min > 1)
            {
                var middle = min + ((max - min) >> 1); // Shift one is the same as dividing by 2

                var interval = list[middle].Interval;

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
        /// Get the index of the first node with an interval the overlaps the query
        /// </summary>
        private static int findLast(Node[] list, IInterval<T> query)
        {
            if (query == null || list.Length == 0)
                return -1;

            int min = -1, max = list.Length;

            while (max - min > 1)
            {
                var middle = min + ((max - min) >> 1); // Shift one is the same as dividing by 2

                var interval = list[middle].Interval;

                var compare = interval.Low.CompareTo(query.High);

                if (compare < 0 || compare == 0 && interval.LowIncluded && query.HighIncluded)
                    min = middle;
                else
                    max = middle;
            }

            return min;
        }

        /// <inheritdoc/>
        public IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            return findOverlaps(_list, query);
        }

        /// <inheritdoc/>
        public bool FindOverlap(IInterval<T> query, out I overlap)
        {
            overlap = null;

            // Check if query overlaps the collection at all
            if (_list == null || !query.Overlaps(Span))
                return false;

            // Find first overlap
            var i = findFirst(_list, query);

            // Check if index is in bound and if the interval overlaps the query
            var result = 0 <= i && i < _list.Length && _list[i].Interval.Overlaps(query);

            if (result)
                overlap = _list[i].Interval;

            return result;
        }

        /// <inheritdoc/>
        public bool FindOverlap(T query, out I overlap)
        {
            return FindOverlap(new IntervalBase<T>(query), out overlap);
        }

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

        #region Gaps

        /// <inheritdoc/>
        public IEnumerable<IInterval<T>> Gaps
        {
            get { return Sorted.Gaps(); }
        }

        /// <inheritdoc/>
        public IEnumerable<IInterval<T>> FindGaps(IInterval<T> query)
        {
            return FindOverlaps(query).Gaps(query);
        }

        #endregion

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
    }
}
