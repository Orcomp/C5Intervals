using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace C5.Intervals
{
    /// <summary>
    /// An implementation of the Layered Containment List by Mikkel Riise Lund using two separate
    /// arrays for intervals and pointers.
    /// </summary>
    /// <typeparam name="I">The interval type.</typeparam>
    /// <typeparam name="T">The interval endpoint type.</typeparam>
    public class LayeredContainmentList<I, T> : CollectionValueBase<I>, IIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Fields

        // Number of intervals in the collection
        private readonly int _count;
        // Number of intervals in the first layer
        private readonly int _firstLayerCount;
        // Number of layers
        private readonly int _layerCount;

        private readonly I[][] _intervalLayers;
        private readonly int[][] _pointerLayers;
        private readonly I[] _sorted;

        private readonly IInterval<T> _span;

        // Maximum Depth
        private int _maximumDepth = -1;
        private IInterval<T> _intervalOfMaximumDepth;

        #endregion

        #region Properties

        /// <summary>
        /// The degree of containment for a collection. This is the length of the longest chain of
        /// intervals strictly contained in each other.
        /// </summary>
        public int ContainmentDegree
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                Contract.Ensures(IsEmpty || Contract.Result<int>() > 0);

                return _layerCount;
            }
        }

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        private void invariant()
        {
            // Layer count is equal to the number of layers of intervals and pointers
            Contract.Invariant(IsEmpty || _layerCount == _intervalLayers.Length);
            Contract.Invariant(IsEmpty || _layerCount == _pointerLayers.Length);
            // The first layer's count is non-negative and at most as big as count
            Contract.Invariant(0 <= _firstLayerCount && _firstLayerCount <= _count);
            // Either the collection is empty or there are one layer or more
            Contract.Invariant(IsEmpty || _layerCount >= 1);
            // Either all intervals are in the first layer, or there are more than one layer
            Contract.Invariant(_count == _firstLayerCount || _layerCount > 1);
            // The layers are null if empty
            Contract.Invariant(!IsEmpty || _intervalLayers == null && _pointerLayers == null);

            // No layer is empty
            Contract.Invariant(IsEmpty || Contract.ForAll(_intervalLayers, layer => layer.Length > 0));
            // Each layer is sorted
            Contract.Invariant(IsEmpty || Contract.ForAll(0, _layerCount, l => Contract.ForAll(0, _intervalLayers[l].Length - 1, i => _intervalLayers[l][i].CompareTo(_intervalLayers[l][i + 1]) <= 0)));
            // Each layer is sorted on both low and high endpoint
            Contract.Invariant(IsEmpty || Contract.ForAll(0, _layerCount, l => Contract.ForAll(0, _intervalLayers[l].Length - 1, i => _intervalLayers[l][i].CompareLow(_intervalLayers[l][i + 1]) <= 0 && _intervalLayers[l][i].CompareHigh(_intervalLayers[l][i + 1]) <= 0)));
            // Each interval in a layer must be contained in at least one interval in each layer below
            Contract.Invariant(IsEmpty ||
                Contract.ForAll(1, _layerCount, ly =>
                    Contract.ForAll(0, ly, lx =>
                        Contract.ForAll(_intervalLayers[ly], y => _intervalLayers[lx].Any(x => x.StrictlyContains(y)))
                    )
                )
            );
        }

        #endregion

        #region Inner Classes

        struct Node
        {
            internal readonly I Interval;
            internal readonly int Pointer;

            internal Node(I interval, int pointer)
                : this()
            {
                Contract.Requires(interval != null);
                Contract.Requires(pointer >= 0);

                Interval = interval;
                Pointer = pointer;
            }

            public override string ToString()
            {
                return Interval.ToString();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a Layered Containment List with a collection of intervals.
        /// </summary>
        /// <param name="intervals">The collection of intervals.</param>
        public LayeredContainmentList(IEnumerable<I> intervals)
        {
            Contract.Ensures(intervals.Count() == Count);

            // TODO: Add to signature
            var isSorted = false;

            // TODO: Calculate the maximum depth during construction based on argument

            // Make intervals to array to allow fast sorting and counting
            _sorted = intervals as I[] ?? intervals.ToArray();

            // Stop if we have no intervals
            if (!_sorted.Any())
                return;

            _count = _sorted.Length;

            // Sort intervals
            if (!isSorted)
            {
                var comparer = IntervalExtensions.CreateComparer<I, T>();
                Sorting.IntroSort(_sorted, 0, _count, comparer);
            }

            var nodeLayers = generateLayers();

            _layerCount = nodeLayers.Count();
            _firstLayerCount = nodeLayers.First.Count;

            // Create the layers for intervals and pointers
            _intervalLayers = new I[_layerCount][];
            _pointerLayers = new int[_layerCount][];

            // Store the count from the layer above
            var previousCount = 0;
            // Create each layer starting at the last layer
            for (var i = _layerCount - 1; i >= 0; i--)
            {
                var count = nodeLayers[i].Count;
                _intervalLayers[i] = new I[count];
                _pointerLayers[i] = new int[count + 1];

                for (var j = 0; j < count; j++)
                {
                    var node = nodeLayers[i][j];
                    _intervalLayers[i][j] = node.Interval;
                    _pointerLayers[i][j] = node.Pointer;
                }

                // Add sentinel pointer
                _pointerLayers[i][count] = previousCount;
                previousCount = count;
            }

            // Cache span value
            _span = new IntervalBase<T>(_intervalLayers.First().First(), _intervalLayers.First()[_firstLayerCount - 1]);
        }

        private ArrayList<ArrayList<Node>> generateLayers()
        {
            // Initialize layers with two empty layers
            var layers = new ArrayList<ArrayList<Node>> { new ArrayList<Node>(), new ArrayList<Node>() };

            // Add the first interval to the first layer to avoid empty layers in the binary search
            if (_count > 0)
                layers[0].Add(new Node(_sorted[0], 0));

            var layer = 0;

            // Insert the rest of the intervals
            for (var i = 1; i < _count; i++)
            {
                var interval = _sorted[i];

                // Search for the layer to insert the interval based on the last layer insert into
                layer = binaryLayerSearch(layers, interval, layer);

                // Add extra layer if needed
                if (layers.Count == layer + 1)
                    layers.Add(new ArrayList<Node>());

                // Add interval and pointer to list
                layers[layer].Add(new Node(interval, layers[layer + 1].Count));
            }

            // Remove empty layer
            layers.Remove();

            return layers;
        }

        private static int binaryLayerSearch(ArrayList<ArrayList<Node>> layers, I interval, int layer)
        {
            Contract.Requires(layers != null);
            Contract.Requires(interval != null);
            Contract.Requires(layers.Last.IsEmpty);
            // The high endpoints of the last interval in each layer is sorted
            Contract.Requires(Contract.ForAll(1, layers.Count - 1, l => layers[l].Last.Interval.CompareHigh(layers[l - 1].Last.Interval) < 0));
            // Layer is non-negative and less than layer count
            Contract.Ensures(0 <= Contract.Result<int>() && Contract.Result<int>() < layers.Count);
            // The last interval in the layer has a high less than or equal to interval's high
            Contract.Ensures(layers[Contract.Result<int>()].IsEmpty || layers[Contract.Result<int>()].Last.Interval.CompareHigh(interval) <= 0);
            // The last interval in the layer below has a high greater than interval's high
            Contract.Ensures(Contract.Result<int>() == 0 || layers[Contract.Result<int>() - 1].Last.Interval.CompareHigh(interval) > 0);

            var low = 0;
            var high = layers.Count - 1;

            do
            {
                var compare = layers[layer].Last.Interval.CompareHigh(interval);

                // interval contains the last interval
                if (compare < 0)
                    high = layer;
                // The last interval contains interval
                else if (compare > 0)
                    low = layer + 1;
                // We have found an interval with the same high
                else
                    return layer;

                // Binarily pick the next layer to check
                layer = low + (high - low >> 2);
            } while (low < high);

            return low;
        }

        #endregion

        #region Collection Value

        /// <inheritdoc/>
        public override bool IsEmpty
        {
            get
            {
                Contract.Ensures(Contract.Result<bool>() == (_count == 0));
                return _count == 0;
            }
        }

        /// <inheritdoc/>
        public override int Count
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() == _count);
                return _count;
            }
        }

        /// <inheritdoc/>
        public override Speed CountSpeed { get { return Speed.Constant; } }

        /// <inheritdoc/>
        public override I Choose()
        {
            if (IsEmpty)
                throw new NoSuchItemException();

            return _intervalLayers.First().First();
        }

        #endregion

        #region Interval Collection

        #region Properties

        #region Data Structure Properties

        // TODO: Support user setting the value
        /// <inheritdoc/>
        public bool AllowsOverlaps { get { return true; } }

        // TODO: Support user setting the value
        /// <inheritdoc/>
        public bool AllowsReferenceDuplicates { get { return true; } }

        #endregion

        #region Collection Properties

        /// <inheritdoc/>
        public IInterval<T> Span { get { return _span; } }

        /// <inheritdoc/>
        public I LowestInterval { get { return _intervalLayers[0][0]; } }

        /// <inheritdoc/>
        public IEnumerable<I> LowestIntervals
        {
            get
            {
                if (IsEmpty)
                    yield break;

                var bottomLayer = _intervalLayers[0];
                var lowestInterval = bottomLayer[0];

                yield return lowestInterval;

                // Iterate through bottom layer as long as the intervals share a low
                for (var i = 1; i < _firstLayerCount; i++)
                {
                    if (bottomLayer[i].CompareLow(lowestInterval) == 0)
                        yield return bottomLayer[i];
                    else
                        yield break;
                }
            }
        }

        /// <inheritdoc/>
        public I HighestInterval { get { return _intervalLayers[0][_firstLayerCount - 1]; } }

        /// <inheritdoc/>
        public IEnumerable<I> HighestIntervals
        {
            get
            {
                if (IsEmpty)
                    yield break;

                var bottomLayer = _intervalLayers[0];
                var highestInterval = bottomLayer[_firstLayerCount - 1];

                yield return highestInterval;

                // Iterate through bottom layer as long as the intervals share a low
                for (var i = _firstLayerCount - 2; i >= 0; i--)
                {
                    if (bottomLayer[i].CompareHigh(highestInterval) == 0)
                        yield return bottomLayer[i];
                    else
                        yield break;
                }
            }
        }

        #region Maximum Depth

        /// <inheritdoc/>
        public int MaximumDepth
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= _layerCount);

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
                Contract.Ensures(_maximumDepth >= _layerCount);

                // If the Maximum Depth is below 0, then the interval of maximum depth has not been set yet
                Contract.Assert(_maximumDepth >= 0 || _intervalOfMaximumDepth == null);

                if (_maximumDepth < 0)
                    _maximumDepth = Sorted.MaximumDepth(ref _intervalOfMaximumDepth);

                return _intervalOfMaximumDepth;
            }
        }

        /*
        /// <summary>
        /// Find the maximum number of overlaps for all intervals overlapping the query interval.
        /// </summary>
        /// <param name="query">The query interval.</param>
        /// <remarks>The query interval is not in the maximum depth. If only one interval
        /// overlaps the query, the result will therefore be 1.</remarks>
        /// <returns>The maximum depth.</returns>
        public int FindMaximumDepth(IInterval<T> query)
        {
            Contract.Requires(query != null);
            Contract.Ensures(Contract.Result<int>() >= 0);

            IInterval<T> intervalOfMaximumDepth = null;
            return FindOverlapsSorted(query).MaximumDepth(ref intervalOfMaximumDepth);
        }

        /// <summary>
        /// Find the maximum depth for all intervals that match the filter and overlap the query interval.
        /// </summary>
        /// <param name="query">The query interval.</param>
        /// <param name="filter">A filter.</param>
        /// <remarks>The query interval is not in the maximum depth. If only one interval
        /// overlaps the query, the result will therefore be 1.</remarks>
        /// <returns>The maximum depth.</returns>
        public int FindMaximumDepth(IInterval<T> query, Func<I, bool> filter)
        {
            Contract.Requires(query != null);
            Contract.Requires(filter != null);
            Contract.Ensures(Contract.Result<int>() >= 0);

            IInterval<T> intervalOfMaximumDepth = null;
            return FindOverlapsSorted(query).Where(filter).MaximumDepth(ref intervalOfMaximumDepth);
        }
        */

        /// <summary>
        /// Find the maximum depth for all intervals that match the filter.
        /// </summary>
        /// <param name="filter">A filter.</param>
        /// <remarks>The query interval is not in the maximum depth. If only one interval
        /// overlaps the query, the result will therefore be 1.</remarks>
        /// <returns>The maximum depth.</returns>
        public int FindMaximumDepth(Func<I, bool> filter)
        {
            Contract.Requires(filter != null);
            Contract.Ensures(Contract.Result<int>() >= 0);

            IInterval<T> intervalOfMaximumDepth = null;
            return Sorted.Where(filter).MaximumDepth(ref intervalOfMaximumDepth);
        }

        #endregion

        #endregion

        #endregion

        #region Enumerable

        /// <inheritdoc/>
        public override IEnumerator<I> GetEnumerator() { return Sorted.GetEnumerator(); }

        /// <inheritdoc/>
        public IEnumerable<I> Sorted { get { return _sorted; } }

        #endregion

        #region Find Overlaps

        /// <inheritdoc/>
        public IEnumerable<I> FindOverlaps(T query)
        {
            return FindOverlaps(new IntervalBase<T>(query));
        }

        /// <inheritdoc/>
        public IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            int layer = 0, lower = 0, upper = _firstLayerCount;

            // Make sure first and last don't point at the same interval (theorem 2)
            while (lower < upper)
            {
                // Cache layer to speed up iteration
                var currentLayer = _intervalLayers[layer];

                var first = lower;

                // The first interval doesn't overlap we need to search for it
                if (!currentLayer[first].Overlaps(query))
                {
                    // We know first doesn't overlap so we can increment it before searching
                    first = findFirst(query, layer, ++first, upper);

                    // If index is out of bound, or found interval doesn't overlap, then the list won't contain any overlaps
                    if (upper <= first || !currentLayer[first].Overlaps(query))
                        yield break;
                }

                // We can use first as lower to minimize search area
                var last = findLast(query, layer, first, upper);

                // Save values for next iteration
                lower = _pointerLayers[layer][first];
                upper = _pointerLayers[layer][last];
                layer++;

                while (first < last)
                    yield return currentLayer[first++];
            }
        }

        // TODO: Decide on using either start/end or lower/upper.
        private int findFirst(IInterval<T> query, int layer, int lower, int upper)
        {
            Contract.Requires(0 <= layer && layer < _intervalLayers.Length);
            Contract.Requires(0 <= lower && lower <= _intervalLayers[layer].Length);
            Contract.Requires(0 <= upper && upper <= _intervalLayers[layer].Length);
            Contract.Requires(query != null);

            // Either the interval at index result overlaps or no intervals in the layer overlap
            Contract.Ensures(Contract.Result<int>() < lower || upper <= Contract.Result<int>() || _intervalLayers[layer][Contract.Result<int>()].Overlaps(query) || Contract.ForAll(lower, upper, i => !_intervalLayers[layer][i].Overlaps(query)));
            // All intervals before index result do not overlap the query
            Contract.Ensures(Contract.ForAll(0, Contract.Result<int>(), i => !_intervalLayers[layer][i].Overlaps(query)));

            int min = lower - 1, max = upper;

            var intervalLayer = _intervalLayers[layer];

            while (min + 1 < max)
            {
                var middle = min + ((max - min) >> 1); // Shift one is the same as dividing by 2

                var interval = intervalLayer[middle];

                var compare = query.Low.CompareTo(interval.High);

                if (compare < 0 || compare == 0 && query.LowIncluded && interval.HighIncluded)
                    max = middle;
                else
                    min = middle;
            }

            return max;
        }

        private int findLast(IInterval<T> query, int layer, int lower, int upper)
        {
            Contract.Requires(0 <= layer && layer < _intervalLayers.Length);
            Contract.Requires(0 <= lower && lower < _intervalLayers[layer].Length);
            Contract.Requires(0 <= upper && upper <= _intervalLayers[layer].Length);
            Contract.Requires(query != null);

            // Either the interval at index result overlaps or no intervals in the layer overlap
            Contract.Ensures(Contract.Result<int>() == 0 || _intervalLayers[layer][Contract.Result<int>() - 1].Overlaps(query) || Contract.ForAll(_intervalLayers[layer], x => !x.Overlaps(query)));
            // All intervals after index result do not overlap the query
            Contract.Ensures(Contract.ForAll(Contract.Result<int>(), _intervalLayers[layer].Count(), i => !_intervalLayers[layer][i].Overlaps(query)));

            int min = lower - 1, max = upper;
            var intervalLayer = _intervalLayers[layer];

            while (min + 1 < max)
            {
                var middle = min + ((max - min) >> 1); // Shift one is the same as dividing by 2

                var interval = intervalLayer[middle];

                var compare = interval.Low.CompareTo(query.High);

                if (compare < 0 || compare == 0 && interval.LowIncluded && query.HighIncluded)
                    min = middle;
                else
                    max = middle;
            }

            return max;
        }

        #region Sorted

        /*
        // TODO: Solve the sorted enumeration problem!
        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection that overlap the query point in sorted order.
        /// </summary>
        /// <param name="query">The query point.</param>
        /// <returns>All intervals that overlap the query point.</returns>
        public IEnumerable<I> FindOverlapsSorted(T query)
        {
            Contract.Requires(query != null);

            var queryInterval = new IntervalBase<T>(query);

            // No overlap if collection is empty or query doesn't overlap collection
            if (IsEmpty || !queryInterval.Overlaps(Span))
                return Enumerable.Empty<I>();

            return findOverlapsSorted(queryInterval);
        }

        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection that overlap the query interval in sorted order.
        /// </summary>
        /// <param name="query">The query interval.</param>
        /// <returns>All intervals that overlap the query interval.</returns>
        public IEnumerable<I> FindOverlapsSorted(IInterval<T> query)
        {
            Contract.Requires(query != null);

            // No overlap if collection is empty or query doesn't overlap collection
            if (IsEmpty || !query.Overlaps(Span))
                return Enumerable.Empty<I>();

            return findOverlapsSorted(query);
        }

        private IEnumerable<I> findOverlapsSorted(IInterval<T> query)
        {
            // TODO: Fix! See Sorted enumerator

            // Create our own stack to avoid stack overflow and to speed up the enumerator
            var stack = new int[_layerCount << 1];
            var i = 0;

            // Keeps track of the index of the first overlap in each layer
            var firstOverlaps = new int[_layerCount];
            for (var l = 0; l < _layerCount; l++)
                firstOverlaps[l] = -1;

            // We stack both values consecutively instead of stacking pairs
            stack[i++] = 0;
            stack[i++] = _firstLayerCount;

            // Continue as long as we still have values on the stack
            while (i > 0)
            {
                // Get start and end from stack
                var end = stack[--i];
                var start = stack[--i];
                var layer = i >> 1;

                // Cache layers for speed
                var intervalLayer = _intervalLayers[layer];
                var pointerLayer = _pointerLayers[layer];

                if (firstOverlaps[layer] < 0)
                    firstOverlaps[layer] = findFirst(query, layer, start, end);

                if (firstOverlaps[layer] >= end)
                    continue;
                if (start < firstOverlaps[layer])
                    start = firstOverlaps[layer];

                // Iterate through all overlaps
                while (start < end && intervalLayer[start].CompareLowHigh(query) <= 0)
                {
                    yield return intervalLayer[start];

                    // If this and the next interval point to different intervals in the next layer, we need to swap layer
                    if (pointerLayer[start] < pointerLayer[start + 1])
                    {
                        // Push the current values
                        stack[i++] = start + 1;
                        stack[i++] = end;
                        // Push the values for the next layer
                        stack[i++] = pointerLayer[start];
                        stack[i++] = pointerLayer[start + 1];
                        break;
                    }

                    start++;
                }
            }
        }
        */

        #endregion

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

            // No overlap if query is null, collection is empty, or query doesn't overlap collection
            if (IsEmpty || !query.Overlaps(Span))
                return false;

            // Find first overlap
            var i = findFirst(query, 0, 0, _firstLayerCount);

            // Check if index is in bound and if the interval overlaps the query
            var result = 0 <= i && i < _firstLayerCount && _intervalLayers[0][i].Overlaps(query);

            if (result)
                overlap = _intervalLayers[0][i];

            return result;
        }

        #endregion

        #region Count Overlaps

        /// <inheritdoc/>
        public int CountOverlaps(T query)
        {
            return countOverlaps(new IntervalBase<T>(query));
        }

        /// <inheritdoc/>
        public int CountOverlaps(IInterval<T> query)
        {
            return countOverlaps(query);
        }

        private int countOverlaps(IInterval<T> query)
        {
            Contract.Requires(query != null);

            if (IsEmpty)
                return 0;

            int layer = 0, lower = 0, upper = _firstLayerCount, count = 0;

            while (lower < upper)
            {
                // The first interval doesn't overlap we need to search for it
                // TODO: Can we tighten the check here? Like i.low < q.high...
                if (!_intervalLayers[layer][lower].Overlaps(query))
                {
                    // We know first doesn't overlap so we can increment it before searching
                    lower = findFirst(query, layer, ++lower, upper);

                    // If index is out of bound, or found interval doesn't overlap, then the layer won't contain any overlaps
                    // TODO: Can we tighten the check here? Like i.low < q.high...
                    if (upper <= lower || !_intervalLayers[layer][lower].Overlaps(query))
                        return count;
                }

                // We can use first as lower to speed up the search
                upper = findLast(query, layer, lower, upper);

                count += upper - lower;

                lower = _pointerLayers[layer][lower];
                upper = _pointerLayers[layer][upper];
                layer++;
            }

            return count;
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

                return _intervalLayers[0].Gaps();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<IInterval<T>> FindGaps(IInterval<T> query)
        {
            return findOverlapsInFirstLayer(query).Gaps(query);
        }

        private IEnumerable<I> findOverlapsInFirstLayer(IInterval<T> query)
        {
            if (IsEmpty)
                yield break;

            var last = _firstLayerCount;

            // We know first doesn't overlap so we can increment it before searching
            var first = findFirst(query, 0, 0, last);

            // Cache variables to speed up iteration
            var currentLayer = _intervalLayers[0];
            I interval;

            while (first < last && (interval = currentLayer[first++]).CompareLowHigh(query) <= 0)
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

        #region GraphViz

        /// <summary>
        /// Get a string representation of the tree in GraphViz dot format.
        /// </summary>
        /// <returns>GraphViz string.</returns>
        public string Graphviz
        {
            get
            {
                return String.Format("digraph LayeredContainmentList {{\n\trankdir=BT;\n\tnode [shape=record];\n\n{0}\n}}", graphviz());
            }
        }

        private string graphviz()
        {
            // Auto-generated contracts to shut static analysis up
            Contract.Requires(IsEmpty || 1 < _intervalLayers.Length);
            Contract.Requires(IsEmpty || _layerCount == _intervalLayers.Length || Count != _firstLayerCount || 0 <= (_firstLayerCount - 1));
            Contract.Requires(IsEmpty || _layerCount == _intervalLayers.Length || Count != _firstLayerCount || 0 >= _firstLayerCount || 0 >= _firstLayerCount || 0 < _intervalLayers.Length);

            var s = String.Empty;

            var layer = 0;
            int lower = 0, upper = _firstLayerCount;

            while (lower < upper)
            {
                var l = new ArrayList<string>();
                var p = String.Empty;
                for (var i = 0; i < upper; i++)
                {
                    l.Add(String.Format("<n{0}> {0}: {1}", i, _intervalLayers[layer][i]));

                    p += String.Format("layer{0}:n{1} -> layer{2}:n{3};\n\t", layer, i, layer + 1, _pointerLayers[layer][i]);
                }

                // Sentinel node
                l.Add(String.Format("<n{0}> {0}: *", upper));
                p += String.Format("layer{0}:n{1} -> layer{2}:n{3};\n\t", layer, upper, layer + 1, _pointerLayers[layer][upper - 1]);

                s += String.Format("\tlayer{0} [fontname=consola, label=\"{1}\"];\n\t{2}\n", layer, String.Join("|", l.ToArray()), p);


                lower = _pointerLayers[layer][lower];
                upper = _pointerLayers[layer][upper];
                layer++;
            }

            s += String.Format("\tlayer{0} [fontname=consola, label=\"<n0> 0: *\"];", layer);

            return s;
        }

        #endregion
    }
}