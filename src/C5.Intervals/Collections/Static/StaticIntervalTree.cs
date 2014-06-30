using System;
using System.Collections.Generic;
using System.Linq;

namespace C5.Intervals
{
    /// <summary>
    /// An implementation of a classic static interval tree as described by Berg et. al in "Computational Geometry - Algorithms and Applications"
    /// </summary>
    /// <typeparam name="I">The interval type.</typeparam>
    /// <typeparam name="T">The interval endpoint type.</typeparam>
    public class StaticIntervalTree<I, T> : CollectionValueBase<I>, IIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Fields

        private readonly Node _root;
        private readonly int _height;
        private readonly int _count;
        private readonly IInterval<T> _span;
        private readonly I _lowestInterval;
        private readonly I _highestInterval;

        private int _maximumDepth = -1;
        // TODO: Expose in a property
        private IInterval<T> _intervalOfMaximumDepth;


        private static readonly IComparer<I> LeftComparer = IntervalExtensions.CreateComparer<I, T>();
        private static readonly IComparer<I> RightComparer = IntervalExtensions.CreateReversedComparer<I, T>();

        #endregion

        #region Inner classes

        private class Node
        {
            #region Fields

            internal readonly T Key;
            // Left and right subtree
            internal readonly Node Left;
            internal readonly Node Right;
            // Left and right list of intersecting intervals for Key
            internal readonly I[] LeftList;
            internal readonly I[] RightList;

            #endregion

            #region Constructor

            internal Node(I[] intervals, ref I lowestInterval, ref I highestInterval, ref int height)
            {
                Key = getKey(intervals);

                IList<I>
                    keyIntersections = new ArrayList<I>(),
                    lefts = null,
                    rights = null;

                // Compute I_mid and construct two sorted lists, LeftList and RightList
                // Divide intervals according to intersection with key
                foreach (var interval in intervals)
                {
                    if (interval.High.CompareTo(Key) < 0)
                    {
                        if (lefts == null)
                            lefts = new ArrayList<I>();

                        lefts.Add(interval);
                    }
                    else if (Key.CompareTo(interval.Low) < 0)
                    {
                        if (rights == null)
                            rights = new ArrayList<I>();

                        rights.Add(interval);
                    }
                    else
                        keyIntersections.Add(interval);
                }

                var count = keyIntersections.Count;
                // Sort intersecting intervals by Low and High
                LeftList = new I[count];
                RightList = new I[count];

                for (var i = 0; i < count; i++)
                    LeftList[i] = RightList[i] = keyIntersections[i];

                // Sort lists
                Sorting.IntroSort(LeftList, 0, count, LeftComparer);
                Sorting.IntroSort(RightList, 0, count, RightComparer);

                // Cache the lowest interval
                if (LeftList[0].CompareLow(lowestInterval) < 0)
                    lowestInterval = LeftList[0];

                // Cache the highest interval
                if (highestInterval.CompareHigh(RightList[0]) < 0)
                    highestInterval = RightList[0];

                var leftHeight = 0;
                var rightHeight = 0;

                // Construct interval tree recursively for Left and Right subtrees
                if (lefts != null)
                    Left = new Node(lefts.ToArray(), ref lowestInterval, ref highestInterval, ref leftHeight);

                if (rights != null)
                    Right = new Node(rights.ToArray(), ref lowestInterval, ref highestInterval, ref rightHeight);

                height = Math.Max(leftHeight, rightHeight) + 1;
            }

            #endregion

            #region Public Methods

            public override string ToString()
            {
                return Key.ToString();
            }

            #endregion
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Create a static interval tree from a collection of intervals.
        /// </summary>
        /// <param name="intervals">Interval collection</param>
        public StaticIntervalTree(IEnumerable<I> intervals)
        {
            var intervalArray = intervals as I[] ?? intervals.ToArray();

            if (!intervalArray.Any())
                return;

            _count = intervalArray.Count();

            // Initialise variables to random interval
            _lowestInterval = _highestInterval = intervalArray[0];

            _root = new Node(intervalArray, ref _lowestInterval, ref _highestInterval, ref _height);
            _span = new IntervalBase<T>(_lowestInterval, _highestInterval);
        }

        #endregion

        #region Median

        private static T getKey(I[] list)
        {
            var length = list.Length;
            var endpoints = new T[length * 2];

            for (var i = 0; i < length; i++)
            {
                var interval = list[i];
                endpoints[i * 2] = interval.Low;
                endpoints[i * 2 + 1] = interval.High;
            }

            return getK(endpoints, list.Count() - 1);
        }

        private static T getK(T[] list, int k)
        {
            list.Shuffle();

            int low = 0, high = list.Length - 1;

            while (high > low)
            {
                var j = partition(list, low, high);

                if (j > k)
                    high = j - 1;
                else if (j < k)
                    low = j + 1;
                else
                    return list[k];
            }

            return list[k];
        }

        private static int partition(T[] list, int low, int high)
        {
            int i = low, j = high + 1;
            var v = list[low];

            while (true)
            {
                while (list[++i].CompareTo(v) < 0)
                    if (i == high)
                        break;
                while (v.CompareTo(list[--j]) < 0)
                    if (j == low)
                        break;
                if (i >= j)
                    break;

                list.Swap(i, j);
            }

            list.Swap(low, j);

            return j;
        }

        #endregion

        #region Collection Value

        /// <inheritdoc/>
        public override bool IsEmpty { get { return _root == null; } }
        /// <inheritdoc/>
        public override int Count { get { return _count; } }
        /// <inheritdoc/>
        public override Speed CountSpeed { get { return Speed.Constant; } }

        /// <inheritdoc/>
        public override I Choose()
        {
            if (_root == null)
                throw new NoSuchItemException();

            return _root.LeftList.First();
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

        #region Span

        /// <inheritdoc/>
        public IInterval<T> Span { get { return _span; } }

        /// <inheritdoc/>
        public I LowestInterval { get { return _lowestInterval; } }

        /// <inheritdoc/>
        public IEnumerable<I> LowestIntervals
        {
            get
            {
                if (IsEmpty)
                    yield break;

                var node = _root;
                while (node != null)
                {
                    foreach (var interval in node.LeftList.TakeWhile(x => x.CompareLow(_lowestInterval) == 0))
                        yield return interval;

                    node = node.Left;
                }
            }
        }

        /// <inheritdoc/>
        public I HighestInterval { get { return _highestInterval; } }

        /// <inheritdoc/>
        public IEnumerable<I> HighestIntervals
        {
            get
            {
                if (IsEmpty)
                    yield break;

                var node = _root;
                while (node != null)
                {
                    foreach (var interval in node.RightList.TakeWhile(x => x.CompareHigh(_highestInterval) == 0))
                        yield return interval;

                    node = node.Right;
                }
            }
        }

        #endregion

        #region Maximum Depth

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

        #endregion

        #endregion

        #endregion

        #region Enumerable

        /// <inheritdoc/>
        public override IEnumerator<I> GetEnumerator()
        {
            return getEnumerator(_root);
        }

        /// <inheritdoc/>
        public IEnumerable<I> Sorted
        {
            get
            {
                if (IsEmpty)
                    yield break;

                // Create queue sorted on high intervals
                var queue = new IntervalHeap<I>(IntervalExtensions.CreateComparer<I, T>());

                // Create a stack
                var stack = new Node[_height];
                var i = 0;

                // Enumerate nodes in order
                var current = _root;
                while (i > 0 || current != null)
                {
                    if (current != null)
                    {
                        stack[i++] = current;

                        // All all intervals in the node
                        queue.AddAll(current.LeftList);

                        current = current.Left;
                    }
                    else
                    {
                        current = stack[--i];

                        // Returns interval before the current node's key
                        while (!queue.IsEmpty && queue.FindMin().Low.CompareTo(current.Key) <= 0)
                            yield return queue.DeleteMin();

                        current = current.Right;
                    }
                }
            }
        }

        private IEnumerable<Node> nodes(Node root)
        {
            if (IsEmpty)
                yield break;

            var stack = new Node[_height];
            var i = 0;

            var current = root;
            while (i > 0 || current != null)
            {
                if (current != null)
                {
                    stack[i++] = current;
                    current = current.Left;
                }
                else
                {
                    current = stack[--i];
                    yield return current;
                    current = current.Right;
                }
            }
        }

        private IEnumerator<I> getEnumerator(Node root)
        {
            foreach (var node in nodes(root))
            {
                // Go through all intervals in the node
                foreach (var interval in node.LeftList)
                    yield return interval;
            }
        }

        #endregion

        #region Find Overlaps

        /// <inheritdoc/>
        public IEnumerable<I> FindOverlaps(T query)
        {
            return findOverlaps(_root, query);
        }

        /// <inheritdoc/>
        public IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            // Break if collection is empty or the query is outside the collections span
            if (IsEmpty || !Span.Overlaps(query))
                yield break;

            var splitNode = _root;
            // Use a lambda instead of out, as out or ref isn't allowed for iterators
            // Find all intersecting intervals in left subtree and right subtree
            foreach (var interval in findSplitNode(_root, query, n => { splitNode = n; }))
                yield return interval;

            foreach (var interval in findLeft(splitNode.Left, query))
                yield return interval;

            foreach (var interval in findRight(splitNode.Right, query))
                yield return interval;
        }

        private static IEnumerable<I> findOverlaps(Node root, T query)
        {
            // Don't search empty leaves
            while (root != null)
            {
                // If query matches root key, we just yield all intervals in root and stop our search
                var compare = query.CompareTo(root.Key);

                if (compare > 0)
                {
                    foreach (var interval in root.RightList.TakeWhile(interval => interval.Overlaps(query)))
                        yield return interval;

                    // Recurse Right
                    root = root.Right;
                }
                // If query comes before root key, we go through LeftList to find all intervals with a Low smaller than our query
                else if (compare < 0)
                {
                    foreach (var interval in root.LeftList.TakeWhile(interval => interval.Overlaps(query)))
                        yield return interval;

                    // Recurse Left
                    root = root.Left;
                }
                else
                {
                    // yield all elements in lists
                    foreach (var interval in root.LeftList.Where(interval => interval.Overlaps(query)))
                        yield return interval;

                    yield break;
                }
            }
        }

        /// <summary>
        /// Create an enumerable, enumerating all intersecting intervals on the path to the split node. Returns the split node in splitNode.
        /// </summary>
        private static IEnumerable<I> findSplitNode(Node root, IInterval<T> query, Action<Node> splitNode)
        {
            while (root != null)
            {
                splitNode(root);

                // Interval is lower than root, go left
                if (query.High.CompareTo(root.Key) < 0)
                {
                    // yield all elements in lists
                    foreach (var interval in root.LeftList.TakeWhile(interval => interval.Overlaps(query)))
                        yield return interval;

                    // Recursively travese left subtree
                    root = root.Left;
                }
                // Interval is higher than root, go right
                else if (root.Key.CompareTo(query.Low) < 0)
                {
                    // yield all elements in lists
                    foreach (var interval in root.RightList.TakeWhile(interval => interval.Overlaps(query)))
                        yield return interval;

                    // Recursively travese right subtree
                    root = root.Right;
                }
                // Otherwise add overlapping nodes in split node
                else
                {
                    // yield all elements in lists
                    foreach (var interval in root.LeftList.Where(interval => interval.Overlaps(query)))
                        yield return interval;

                    yield break;
                }
            }
        }

        private IEnumerable<I> findLeft(Node root, IInterval<T> query)
        {
            // If root is null we have reached the end
            while (root != null)
            {
                //
                if (root.Key.CompareTo(query.Low) < 0)
                {
                    // Add all intersecting intervals from right list
                    // yield all elements in lists
                    foreach (var interval in root.RightList.TakeWhile(interval => interval.Overlaps(query)))
                        yield return interval;

                    // Travese right subtree
                    root = root.Right;
                }
                //
                else if (query.Low.CompareTo(root.Key) < 0)
                {
                    // As our query interval contains the interval [root.Key:splitNode]
                    // all intervals in root can be returned without any checks

                    // yield all elements in lists
                    foreach (var interval in root.RightList.TakeWhile(interval => interval.Overlaps(query)))
                        yield return interval;

                    // Recursively add all intervals in right subtree as they must be
                    // contained by [root.Key:splitNode]
                    var child = getEnumerator(root.Right);
                    while (child.MoveNext())
                        yield return child.Current;

                    // Travese left subtree
                    root = root.Left;
                }
                else
                {
                    // Add all intersecting intervals from right list
                    // yield all elements in lists
                    foreach (var interval in root.RightList.TakeWhile(interval => interval.Overlaps(query)))
                        yield return interval;

                    // If we find the matching node, we can add everything in the left subtree
                    var child = getEnumerator(root.Right);
                    while (child.MoveNext())
                        yield return child.Current;

                    yield break;
                }
            }
        }

        private IEnumerable<I> findRight(Node root, IInterval<T> query)
        {
            while (root != null)
            {
                //
                if (query.High.CompareTo(root.Key) < 0)
                {
                    // Add all intersecting intervals from left list
                    foreach (var interval in root.LeftList.TakeWhile(interval => interval.Overlaps(query)))
                        yield return interval;

                    // Travese left subtree
                    root = root.Left;
                }
                //
                else if (root.Key.CompareTo(query.High) < 0)
                {
                    // As our query interval contains the interval [root.Key:splitNode]
                    // all intervals in root can be returned without any checks
                    foreach (var interval in root.RightList.TakeWhile(interval => interval.Overlaps(query)))
                        yield return interval;

                    // Recursively add all intervals in right subtree as they must be
                    // contained by [root.Key:splitNode]
                    var child = getEnumerator(root.Left);
                    while (child.MoveNext())
                        yield return child.Current;

                    // Travese right subtree
                    root = root.Right;
                }
                else
                {
                    // Add all intersecting intervals from left list
                    foreach (var interval in root.LeftList.TakeWhile(interval => interval.Overlaps(query)))
                        yield return interval;

                    // If we find the matching node, we can add everything in the left subtree
                    var child = getEnumerator(root.Left);
                    while (child.MoveNext())
                        yield return child.Current;

                    yield break;
                }
            }
        }

        #endregion

        #region Find Overlap

        /// <inheritdoc/>
        public bool FindOverlap(T query, out I overlap)
        {
            var node = _root;

            while (node != null)
            {
                var compare = query.CompareTo(node.Key);

                if (compare < 0)
                {
                    // Check if first interval in low sorted list overlaps endpoint
                    compare = (overlap = node.LeftList[0]).Low.CompareTo(query);
                    if (compare < 0 || compare == 0 && overlap.LowIncluded)
                        return true;

                    node = node.Left;
                }
                else if (compare > 0)
                {
                    // Check if first interval in high sorted list overlaps endpoint
                    compare = (overlap = node.RightList[0]).High.CompareTo(query);
                    if (compare > 0 || compare == 0 && overlap.HighIncluded)
                        return true;

                    node = node.Right;
                }
                else
                {
                    foreach (var interval in node.LeftList.Where(interval => interval.Overlaps(query)))
                    {
                        overlap = interval;
                        return true;
                    }
                    break;
                }
            }

            overlap = null;
            return false;
        }

        /// <inheritdoc/>
        public bool FindOverlap(IInterval<T> query, out I overlap)
        {
            bool result;

            using (var enumerator = FindOverlaps(query).GetEnumerator())
                overlap = (result = enumerator.MoveNext()) ? enumerator.Current : null;

            return result;
        }

        #endregion

        #region Count Overlaps

        /// <inheritdoc/>
        public int CountOverlaps(T query)
        {
            var node = _root;
            var count = 0;

            while (node != null)
            {
                var compare = query.CompareTo(node.Key);

                if (compare < 0)
                {
                    count += node.LeftList.TakeWhile(x => x.CompareLow(query) <= 0).Count();

                    node = node.Left;
                }
                else if (compare > 0)
                {
                    count += node.RightList.TakeWhile(x => x.CompareHigh(query) >= 0).Count();

                    node = node.Right;
                }
                else
                {
                    I interval;
                    var i = 0;
                    while (true)
                    {
                        compare = (interval = node.LeftList[i++]).CompareLow(query);

                        if (compare < 0)
                        {
                            if (interval.CompareHigh(query) >= 0)
                                count++;
                        }
                        else if (compare == 0)
                            count++;
                        else
                            break;
                    }

                    break;
                }
            }

            return count;
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
                return Sorted.Gaps();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<IInterval<T>> FindGaps(IInterval<T> query)
        {
            return FindOverlaps(query).Gaps(query, false);
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

        #region Formatting

        /// <summary>
        /// Print the tree structure in Graphviz format
        /// </summary>
        /// <returns></returns>
        public string Graphviz
        {
            get
            {
                return "digraph StaticIntervalTree {\n"
                    + "\troot [fontname=consolas,shape=plaintext,label=\"Root\"];\n"
                    + graphviz(_root, "root")
                    + "}\n";
            }
        }

        private int _nodeCounter;
        private int _nullCounter;

        private string graphviz(Node root, string parent)
        {
            // Leaf
            int id;
            if (root == null)
            {
                id = _nullCounter++;
                return
                    String.Format("\tleaf{0} [shape=point];\n", id) +
                    String.Format("\t{0} -> leaf{1};\n", parent, id);
            }

            id = _nodeCounter++;
            return
                String.Format("\tnode{0} [fontname=consolas,label=\"{1}\"];\n", id, root.Key) +
                String.Format("\t{0} -> node{1};\n", parent, id) +
                graphviz(root.Left, "node" + id) +
                String.Format("\tnode{0}left [fontname=consolas,shape=plaintext, label=\"{1}\"];\n", id, root.LeftList) +
                String.Format("\tnode{0} -> node{0}left [style=dotted];\n", id) +
                String.Format("\tnode{0}right [fontname=consolas,shape=plaintext, label=\"{1}\"];\n", id, root.RightList.Reverse()) +
                String.Format("\tnode{0} -> node{0}right [style=dotted];\n", id) +
                graphviz(root.Right, "node" + id);
        }

        #endregion
    }

    /// <summary>
    /// Extension methods used to find the median endpoint
    /// </summary>
    static class StaticIntervalTreeExtensions
    {
        /// <summary>
        /// Randomly shuffles a list in linear time
        /// </summary>
        /// <param name="list">The list</param>
        /// <typeparam name="T">The element type</typeparam>
        public static void Shuffle<T>(this T[] list)
        {
            var random = new Random();
            var n = list.Length;
            while (--n > 0)
                list.Swap(random.Next(n + 1), n);
        }

        /// <summary>
        /// Swaps two elements in a list
        /// </summary>
        /// <param name="list">The list</param>
        /// <param name="i">The index of the first element</param>
        /// <param name="j">The index of the second element</param>
        /// <typeparam name="T">The element type</typeparam>
        public static void Swap<T>(this T[] list, int i, int j)
        {
            var tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }
}
