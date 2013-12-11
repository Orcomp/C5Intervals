﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace C5.intervals
{
    // TODO: Document reference equality duplicates
    /// <summary>
    /// An implementation of the Interval Binary Search Tree as described by Hanson et. al in "The IBS-Tree: A Data Structure for Finding All Intervals That Overlap a Point" using an Red-Black tree balancing scheme.
    /// </summary>
    /// <typeparam name="I">The interval type.</typeparam>
    /// <typeparam name="T">The interval endpoint type.</typeparam>
    public class IntervalBinarySearchTree<I, T> : CollectionValueBase<I>, IIntervalCollection<I, T>
        where I : IInterval<T>
        where T : IComparable<T>
    {
        private const bool RED = true;
        private const bool BLACK = false;

        private Node _root;
        private int _count;

        private static readonly IEqualityComparer<I> Comparer = ComparerFactory<I>.CreateEqualityComparer((x, y) => ReferenceEquals(x, y), x => x.GetHashCode());

        #region Code Contracts


        #endregion

        #region Red-black tree helper methods

        private static bool isRed(Node node)
        {
            // Null nodes are by convention black
            return node != null && node.Color == RED;
        }

        private static Node rotate(Node root)
        {
            if (isRed(root.Right) && !isRed(root.Left))
                root = rotateLeft(root);
            if (isRed(root.Left) && isRed(root.Left.Left))
                root = rotateRight(root);
            if (isRed(root.Left) && isRed(root.Right))
            {
                root.Color = RED;
                root.Left.Color = root.Right.Color = BLACK;
            }

            return root;
        }

        private static Node rotateRight(Node root)
        {
            Contract.Requires(root != null);
            Contract.Requires(root.Left != null);

            // Rotate
            var node = root.Left;
            root.Left = node.Right;
            node.Right = root;
            node.Color = root.Color;
            root.Color = RED;


            // 1
            node.Less.AddAll(root.Less);
            node.Equal.AddAll(root.Less);

            // 2
            var between = node.Greater - root.Greater;
            root.Less.AddAll(between);
            node.Greater.RemoveAll(between);

            // 3
            root.Equal.RemoveAll(node.Greater);
            root.Greater.RemoveAll(node.Greater);

            root.UpdateMaximumDepth();
            node.UpdateMaximumDepth();

            return node;
        }

        private static Node rotateLeft(Node root)
        {
            Contract.Requires(root != null);
            Contract.Requires(root.Right != null);

            // Rotate
            var node = root.Right;
            root.Right = node.Left;
            node.Left = root;
            node.Color = root.Color;
            root.Color = RED;


            // 1
            node.Greater.AddAll(root.Greater);
            node.Equal.AddAll(root.Greater);

            // 2
            var between = node.Less - root.Less;
            root.Greater.AddAll(between);
            node.Less.RemoveAll(between);

            // 3
            root.Equal.RemoveAll(node.Less);
            root.Less.RemoveAll(node.Less);


            // Update PMO
            root.UpdateMaximumDepth();
            node.UpdateMaximumDepth();

            return node;
        }

        #endregion

        #region Node nested classes

        class Node
        {
            public T Key { get; private set; }

            private IntervalSet _less;
            private IntervalSet _equal;
            private IntervalSet _greater;

            public IntervalSet Less
            {
                get { return _less ?? (_less = new IntervalSet()); }
            }
            public IntervalSet Equal
            {
                get { return _equal ?? (_equal = new IntervalSet()); }
            }
            public IntervalSet Greater
            {
                get { return _greater ?? (_greater = new IntervalSet()); }
            }

            public Node Left { get; internal set; }
            public Node Right { get; internal set; }

            public int Delta { get; internal set; }
            public int DeltaAfter { get; internal set; }
            private int Sum { get; set; }
            public int Max { get; private set; }

            public bool Color { get; set; }

            public int Balance
            {
                get { return -1; }
                set { throw new NotImplementedException(); }
            }

            public Node(T key)
            {
                Key = key;
                Color = RED;
            }

            public void UpdateMaximumDepth()
            {
                // Set Max to Left's Max
                Max = Left != null ? Left.Max : 0;

                // Start building up the other possible Max sums
                var value = (Left != null ? Left.Sum : 0) + Delta;
                // And check if they are higher the previously found max
                if (value > Max)
                    Max = value;

                // Add DeltaAfter and check for new max
                value += DeltaAfter;
                if (value > Max)
                    Max = value;

                // Save the sum value using the previous calculations
                Sum = value + (Right != null ? Right.Sum : 0);

                // Add Right's max and check for new max
                value += Right != null ? Right.Max : 0;
                if (value > Max)
                    Max = value;
            }

            public override string ToString()
            {
                return String.Format("{0}-{1}", Color ? "R" : "B", Key);
            }
        }

        private sealed class IntervalSet : HashSet<I>
        {
            private IntervalSet(IEnumerable<I> intervals)
                : base(Comparer)
            {
                AddAll(intervals);
            }

            public IntervalSet()
                : base(Comparer)
            {
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                var s = new ArrayList<string>();

                foreach (var interval in this)
                    s.Add(interval.ToString());

                return s.IsEmpty ? String.Empty : String.Join(", ", s.ToArray());
            }

            /// <summary>
            /// The intersection of two interval sets.
            /// </summary>
            /// <param name="s1">First set.</param>
            /// <param name="s2">Second set.</param>
            /// <returns>The set intersection of the sets.</returns>
            public static IntervalSet operator -(IntervalSet s1, IntervalSet s2)
            {
                Contract.Requires(s1 != null);
                Contract.Requires(s2 != null);

                var res = new IntervalSet(s1);
                res.RemoveAll(s2);
                return res;
            }

            /// <summary>
            /// The union of two interval sets.
            /// </summary>
            /// <param name="s1">First set.</param>
            /// <param name="s2">Second set.</param>
            /// <returns>The set union of the sets.</returns>
            public static IntervalSet operator +(IntervalSet s1, IntervalSet s2)
            {
                Contract.Requires(s1 != null);
                Contract.Requires(s2 != null);

                var res = new IntervalSet(s1);
                res.AddAll(s2);
                return res;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create an Interval Binary Search Tree with a collection of intervals.
        /// </summary>
        /// <param name="intervals">The collection of intervals.</param>
        public IntervalBinarySearchTree(IEnumerable<I> intervals)
        {
            foreach (var interval in intervals)
                Add(interval);
        }

        /// <summary>
        /// Create empty Interval Binary Search Tree.
        /// </summary>
        public IntervalBinarySearchTree()
        {
        }

        #endregion

        #region ICollection, IExtensible

        #region insertion

        private Node addLow(Node root, Node right, I interval)
        {
            if (root == null)
                root = new Node(interval.Low);

            var compareTo = root.Key.CompareTo(interval.Low);

            if (compareTo < 0)
            {
                root.Right = addLow(root.Right, right, interval);
            }
            else if (compareTo == 0)
            {
                // If everything in the right subtree of root will lie within the interval
                if (right != null && right.Key.CompareTo(interval.High) <= 0)
                    root.Greater.Add(interval);

                if (interval.LowIncluded)
                    root.Equal.Add(interval);

                // Update delta
                if (interval.LowIncluded)
                    root.Delta++;
                else
                    root.DeltaAfter++;
            }
            else if (compareTo > 0)
            {
                // Everything in the right subtree of root will lie within the interval
                if (right != null && right.Key.CompareTo(interval.High) <= 0)
                    root.Greater.Add(interval);

                // root key is between interval.low and interval.high
                if (root.Key.CompareTo(interval.High) < 0)
                    root.Equal.Add(interval);

                // TODO: Figure this one out: if (interval.low != -inf.)
                root.Left = addLow(root.Left, root, interval);
            }

            // Red Black tree rotations
            root = rotate(root);

            // Update PMO
            root.UpdateMaximumDepth();

            return root;
        }

        private Node addHigh(Node root, Node left, I interval)
        {
            if (root == null)
                root = new Node(interval.High);

            var compareTo = root.Key.CompareTo(interval.High);

            if (compareTo > 0)
            {
                root.Left = addHigh(root.Left, left, interval);
            }
            else if (compareTo == 0)
            {
                // If everything in the right subtree of root will lie within the interval
                if (left != null && left.Key.CompareTo(interval.Low) >= 0)
                    root.Less.Add(interval);

                if (interval.HighIncluded)
                    root.Equal.Add(interval);

                if (!interval.HighIncluded)
                    root.Delta--;
                else
                    root.DeltaAfter--;
            }
            else if (compareTo < 0)
            {
                // Everything in the right subtree of root will lie within the interval
                if (left != null && left.Key.CompareTo(interval.Low) >= 0)
                    root.Less.Add(interval);

                // root key is between interval.low and interval.high
                if (root.Key.CompareTo(interval.Low) > 0)
                    root.Equal.Add(interval);

                // TODO: Figure this one out: if (interval.low != -inf.)
                root.Right = addHigh(root.Right, root, interval);
            }

            // Red Black tree rotations
            root = rotate(root);

            // TODO: Figure out if this is still the correct place to put update
            // Update PMO
            root.UpdateMaximumDepth();

            return root;
        }

        /// <inheritdoc/>
        public bool IsReadOnly { get { return false; } }

        /// <inheritdoc/>
        public bool Add(I interval)
        {
            // TODO: Add event!

            _root = addLow(_root, null, interval);
            _root = addHigh(_root, null, interval);

            _root.Color = BLACK;

            return true;
        }

        /// <inheritdoc/>
        public void AddAll(IEnumerable<I> intervals)
        {
            foreach (var interval in intervals)
                Add(interval);
        }

        #endregion

        #region deletion

        private Node removeByLow(Node root, Node right, I interval)
        {
            if (root == null)
                return null;

            var compareTo = root.Key.CompareTo(interval.Low);

            if (compareTo < 0)
            {
                root.Right = removeByLow(root.Right, right, interval);
            }
            else if (compareTo == 0)
            {
                // Remove interval from Greater set
                if (right != null && right.Key.CompareTo(interval.High) <= 0)
                    root.Greater.Remove(interval);

                // Remove interval from Equal set
                if (interval.LowIncluded)
                    root.Equal.Remove(interval);

                // Update delta
                if (interval.LowIncluded)
                    root.Delta--;
                else
                    root.DeltaAfter--;
            }
            else if (compareTo > 0)
            {
                // Everything in the right subtree of root will lie within the interval
                if (right != null && right.Key.CompareTo(interval.High) <= 0)
                    root.Greater.Remove(interval);

                // root key is between interval.low and interval.high
                if (root.Key.CompareTo(interval.High) < 0)
                    root.Equal.Remove(interval);

                // TODO: Figure this one out: if (interval.low != -inf.)
                root.Left = removeByLow(root.Left, root, interval);
            }

            // Update PMO
            root.UpdateMaximumDepth();

            return root;
        }

        private Node removeByHigh(Node root, Node left, I interval)
        {
            if (root == null)
                return null;

            var compareTo = root.Key.CompareTo(interval.High);

            if (compareTo > 0)
            {
                root.Left = removeByHigh(root.Left, left, interval);
            }
            else if (compareTo == 0)
            {
                // If everything in the right subtree of root will lie within the interval
                if (left != null && left.Key.CompareTo(interval.Low) >= 0)
                    root.Less.Remove(interval);

                if (interval.HighIncluded)
                    root.Equal.Remove(interval);

                if (!interval.HighIncluded)
                    root.Delta++;
                else
                    root.DeltaAfter++;
            }
            else if (compareTo < 0)
            {
                // Everything in the right subtree of root will lie within the interval
                if (left != null && left.Key.CompareTo(interval.Low) >= 0)
                    root.Less.Remove(interval);

                // root key is between interval.low and interval.high
                if (root.Key.CompareTo(interval.Low) > 0)
                    root.Equal.Remove(interval);

                // TODO: Figure this one out: if (interval.low != -inf.)
                root.Right = removeByHigh(root.Right, root, interval);
            }
            // Update PMO
            root.UpdateMaximumDepth();

            return root;
        }

        private bool notAlone(Node root, T endpoint)
        {
            Contract.Requires(root != null);

            Node nodeForEndpoint = findNode(root, endpoint);

            if (nodeForEndpoint != null)
            {
                foreach (IInterval<T> interval in nodeForEndpoint.Greater)
                {
                    if (interval.Low.Equals(endpoint) || interval.High.Equals(endpoint))
                    {
                        return true;
                    }
                }

                foreach (IInterval<T> interval in nodeForEndpoint.Less)
                {
                    if (interval.Low.Equals(endpoint) || interval.High.Equals(endpoint))
                    {
                        return true;
                    }
                }

                foreach (IInterval<T> interval in nodeForEndpoint.Equal)
                {
                    if (interval.Low.Equals(endpoint) || interval.High.Equals(endpoint))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private Node findNode(Node root, T endpoint)
        {
            Contract.Requires(root != null);

            if (root.Key.CompareTo(endpoint) == 0)
            {
                return root;
            }

            if (root.Key.CompareTo(endpoint) < 0)
            {
                return findNode(root.Right, endpoint);
            }

            if (root.Key.CompareTo(endpoint) > 0)
            {
                return findNode(root.Left, endpoint);
            }

            return null;
        }

        // Flip the colors of a node and its two children
        private void flipColors(Node h)
        {
            Contract.Requires(h != null);

            // h must have opposite color of its two children
            if ((h != null) && (h.Left != null) && (h.Right != null)
                && ((!isRed(h) && isRed(h.Left) && isRed(h.Right))
                || (isRed(h) && !isRed(h.Left) && !isRed(h.Right))))
            {
                h.Color = !h.Color;
                h.Left.Color = !h.Left.Color;
                h.Right.Color = !h.Right.Color;
            }
        }

        // Assuming that h is red and both h.left and h.left.left
        // are black, make h.left or one of its children red.
        private Node moveRedLeft(Node h)
        {
            flipColors(h);
            if (isRed(h.Right.Left))
            {
                h.Right = rotateRight(h.Right);
                h = rotateLeft(h);
                // flipColors(h);
            }
            return h;
        }

        // Assuming that h is red and both h.right and h.right.left
        // are black, make h.right or one of its children red.
        private Node moveRedRight(Node h)
        {
            flipColors(h);
            if (isRed(h.Left.Left))
            {
                h = rotateRight(h);
                // flipColors(h);
            }
            return h;
        }

        // 
        private Node deleteNode(Node h, Node toRemove)
        {
            if (h.Left == toRemove)
            {
                h.Left = null;
                return null;
            }

            if (!isRed(h.Left) && !isRed(h.Left.Left))
                h = moveRedLeft(h);

            h.Left = deleteNode(h.Left, toRemove);

            return rotate(h);
        }

        // the smallest key; null if no such key
        private T min()
        {
            return min(_root).Key;
        }

        // the smallest key in subtree rooted at x; null if no such key
        private Node min(Node x)
        {
            if (x.Left == null)
                return x;

            return min(x.Left);
        }

        // delete the key-value pair with the given key
        private void remove(T endpoint)
        {

            // if both children of root are black, set root to red
            if (!isRed(_root.Left) && !isRed(_root.Right))
                _root.Color = RED;

            _root = remove(_root, null, false, endpoint);
            // TODO: if (!isEmpty()) root.color = BLACK;
        }

        // delete the key-value pair with the given key rooted at h
        private Node remove(Node root, Node parent, bool left, T endpoint)
        {

            if (endpoint.CompareTo(root.Key) < 0)
            {
                if (!isRed(root.Left) && !isRed(root.Left.Left))
                {
                    if (isRed(root))
                        root = moveRedLeft(root);
                }
                root.Left = remove(root.Left, root, true, endpoint);
            }
            else
            {
                if (isRed(root.Left))
                    root = rotateRight(root);
                if (endpoint.CompareTo(root.Key) == 0 && (root.Right == null))
                    return null;
                if (!isRed(root.Right) && !isRed(root.Right.Left))
                    if (isRed(root))
                        root = moveRedRight(root);
                if (endpoint.CompareTo(root.Key) == 0)
                {
                    // Save the Greater and CheckIBSLessInvariant set of root
                    //IntervalSet rootGreater = root.Greater;
                    //IntervalSet rootLess = root.CheckIBSLessInvariant;

                    // Save key and sets of right child's minimum
                    Node minChild = min(root.Right);
                    IntervalSet minGreater = minChild.Greater;
                    IntervalSet minLess = minChild.Less;
                    IntervalSet minEqual = minChild.Equal;

                    // Make new node with the Key of the right child's minimum
                    var node = new Node(minChild.Key) { Left = root.Left, Right = root.Right };
                    node.Greater.AddAll(minGreater);
                    node.Less.AddAll(minLess);
                    node.Equal.AddAll(minEqual);

                    node.Greater.AddAll(root.Greater);
                    node.Less.AddAll(root.Less);
                    node.Equal.AddAll(root.Equal);

                    // Update deltas
                    node.Delta = root.Delta + minChild.Delta;
                    node.DeltaAfter = root.DeltaAfter + minChild.DeltaAfter;

                    if (parent == null)
                        _root = node;
                    else
                    {
                        if (left)
                            parent.Left = node;
                        else parent.Right = node;
                    }

                    deleteNode(root.Right, minChild);

                    return rotate(node);
                }
                root.Right = remove(root.Right, root, false, endpoint);
            }
            return rotate(root);
        }

        /// <inheritdoc/>
        public bool Remove(I interval)
        {
            // Delete the interval from the sets
            _root = removeByLow(_root, null, interval);
            _root = removeByHigh(_root, null, interval);

            // If no other interval has the same endpoint, delete the endpoint
            if (!notAlone(_root, interval.Low))
            {
                // Delete endpoint
                remove(interval.Low);
            }

            if (!notAlone(_root, interval.High))
            {
                // Delete endpoint
                remove(interval.High);
            }

            return true;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            _root = null;
            _count = 0;
            // TODO: Add what ever is missing
        }

        /// <inheritdoc/>
        public override bool IsEmpty { get { return _root == null; } }

        // TODO: Implement
        /// <inheritdoc/>
        public override int Count
        {
            get { return this.Count(); }
        }

        /// <inheritdoc/>
        public override Speed CountSpeed { get { return Speed.Constant; } }

        #endregion


        #region IEnumerable

        /// <inheritdoc/>
        public override IEnumerator<I> GetEnumerator()
        {
            var set = new IntervalSet();

            var enumerator = getEnumerator(_root);
            while (enumerator.MoveNext())
                set.Add(enumerator.Current);

            return set.GetEnumerator();
        }

        #endregion

        #region ICollectionValue

        /// <inheritdoc/>
        public override I Choose()
        {
            if (_root == null)
                throw new NoSuchItemException();

            return (_root.Less + _root.Equal + _root.Greater).Choose();
        }

        #endregion

        #region IIntervaled

        /// <inheritdoc/>
        public IInterval<T> Span
        {
            get
            {
                return new IntervalBase<T>(getLowest(_root), getHighest(_root));
            }
        }

        private static IInterval<T> getLowest(Node root)
        {
            Contract.Requires(root != null);

            if (!root.Less.IsEmpty)
                return root.Less.Choose();

            if (root.Left != null)
                return getLowest(root.Left);

            return !root.Equal.IsEmpty ? root.Equal.Choose() : root.Greater.Choose();
        }

        private static IInterval<T> getHighest(Node root)
        {
            Contract.Requires(root != null);

            if (!root.Greater.IsEmpty)
                return root.Greater.Choose();

            if (root.Right != null)
                return getHighest(root.Right);

            return !root.Equal.IsEmpty ? root.Equal.Choose() : root.Less.Choose();
        }

        /// <inheritdoc/>
        public IEnumerable<I> FindOverlaps(T query)
        {
            // TODO: Add checks for span overlap, null query, etc.
            return findOverlap(_root, query);
        }

        private static IEnumerable<I> findOverlap(Node root, T query)
        {
            // Search the tree until we reach the bottom of the tree    
            while (root != null)
            {
                // Store compare value as we need it twice
                var compareTo = query.CompareTo(root.Key);

                // Query is to the left of the current node
                if (compareTo < 0)
                {
                    // Return all intervals in CheckIBSLessInvariant
                    foreach (var interval in root.Less)
                        yield return interval;

                    // Move left
                    root = root.Left;
                }
                // Query is to the right of the current node
                else if (0 < compareTo)
                {
                    // Return all intervals in Greater
                    foreach (var interval in root.Greater)
                        yield return interval;

                    // Move right
                    root = root.Right;
                }
                // Node with query value found
                else
                {
                    // Return all intervals in Equal
                    foreach (var interval in root.Equal)
                        yield return interval;

                    // Stop as the search is done
                    yield break;
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            if (ReferenceEquals(query, null))
                yield break;

            // Break if collection is empty or the query is outside the collections span
            if (IsEmpty || !Span.Overlaps(query))
                yield break;

            var set = new IntervalSet();

            var splitNode = _root;
            // Use a lambda instead of out, as out or ref isn't allowed for itorators
            set.AddAll(findSplitNode(_root, query, n => { splitNode = n; }));

            // Find all intersecting intervals in left subtree
            if (query.Low.CompareTo(splitNode.Key) < 0)
                set.AddAll(findLeft(splitNode.Left, query));

            // Find all intersecting intervals in right subtree
            if (splitNode.Key.CompareTo(query.High) < 0)
                set.AddAll(findRight(splitNode.Right, query));

            foreach (var interval in set)
                yield return interval;
        }

        /// <summary>
        /// Create an enumerable, enumerating all intersecting intervals on the path to the split node. Returns the split node in splitNode.
        /// </summary>
        private static IEnumerable<I> findSplitNode(Node root, IInterval<T> query, Action<Node> splitNode)
        {
            if (root == null) yield break;

            splitNode(root);

            // Interval is lower than root, go left
            if (query.High.CompareTo(root.Key) < 0)
            {
                foreach (var interval in root.Less)
                    yield return interval;

                // Recursively travese left subtree
                foreach (var interval in findSplitNode(root.Left, query, splitNode))
                    yield return interval;
            }
            // Interval is higher than root, go right
            else if (root.Key.CompareTo(query.Low) < 0)
            {
                foreach (var interval in root.Greater)
                    yield return interval;

                // Recursively travese right subtree
                foreach (var interval in findSplitNode(root.Right, query, splitNode))
                    yield return interval;
            }
            // Otherwise add overlapping nodes in split node
            else
            {
                foreach (var interval in root.Less + root.Equal + root.Greater)
                    if (query.Overlaps(interval))
                        yield return interval;
            }
        }

        private IEnumerable<I> findLeft(Node root, IInterval<T> query)
        {
            // If root is null we have reached the end
            if (root == null) yield break;

            var compareTo = query.Low.CompareTo(root.Key);

            //
            if (compareTo > 0)
            {
                foreach (var interval in root.Greater)
                    yield return interval;

                // Recursively travese right subtree
                foreach (var interval in findLeft(root.Right, query))
                    yield return interval;
            }
            //
            else if (compareTo < 0)
            {
                foreach (var interval in root.Less + root.Equal + root.Greater)
                    yield return interval;

                // Recursively add all intervals in right subtree as they must be
                // contained by [root.Key:splitNode]
                var child = getEnumerator(root.Right);
                while (child.MoveNext())
                    yield return child.Current;

                // Recursively travese left subtree
                foreach (var interval in findLeft(root.Left, query))
                    yield return interval;
            }
            else
            {
                // Add all intersecting intervals from right list
                foreach (var interval in root.Greater)
                    yield return interval;

                if (query.LowIncluded)
                    foreach (var interval in root.Equal)
                        yield return interval;

                // If we find the matching node, we can add everything in the left subtree
                var child = getEnumerator(root.Right);
                while (child.MoveNext())
                    yield return child.Current;
            }
        }

        private IEnumerable<I> findRight(Node root, IInterval<T> query)
        {
            // If root is null we have reached the end
            if (root == null) yield break;

            var compareTo = query.High.CompareTo(root.Key);

            //
            if (compareTo < 0)
            {
                // Add all intersecting intervals from left list
                foreach (var interval in root.Less)
                    yield return interval;

                // Otherwise Recursively travese left subtree
                foreach (var interval in findRight(root.Left, query))
                    yield return interval;
            }
            //
            else if (compareTo > 0)
            {
                // As our query interval contains the interval [root.Key:splitNode]
                // all intervals in root can be returned without any checks
                foreach (var interval in root.Less + root.Equal + root.Greater)
                    yield return interval;

                // Recursively add all intervals in right subtree as they must be
                // contained by [root.Key:splitNode]
                var child = getEnumerator(root.Left);
                while (child.MoveNext())
                    yield return child.Current;

                // Recursively travese left subtree
                foreach (var interval in findRight(root.Right, query))
                    yield return interval;
            }
            else
            {
                // Add all intersecting intervals from left list
                foreach (var interval in root.Less)
                    yield return interval;

                if (query.HighIncluded)
                    foreach (var interval in root.Equal)
                        yield return interval;

                // If we find the matching node, we can add everything in the left subtree
                var child = getEnumerator(root.Left);
                while (child.MoveNext())
                    yield return child.Current;
            }
        }


        private IEnumerator<I> getEnumerator(Node node)
        {
            // Just return if tree is empty
            if (node == null) yield break;

            // Recursively retrieve intervals in left subtree
            if (node.Left != null)
            {
                var child = getEnumerator(node.Left);

                while (child.MoveNext())
                    yield return child.Current;
            }

            // Go through all intervals in the node
            foreach (var interval in node.Less + node.Equal + node.Greater)
                yield return interval;

            // Recursively retrieve intervals in right subtree
            if (node.Right != null)
            {
                var child = getEnumerator(node.Right);

                while (child.MoveNext())
                    yield return child.Current;
            }

        }

        /// <inheritdoc/>
        public bool FindOverlap(IInterval<T> query, ref I overlap)
        {
            bool result;

            using (var enumerator = FindOverlaps(query).GetEnumerator())
            {
                result = enumerator.MoveNext();

                if (result)
                    overlap = enumerator.Current;
            }

            return result;
        }

        /// <inheritdoc/>
        public bool FindOverlap(T query, ref I overlap)
        {
            bool result;

            using (var enumerator = FindOverlaps(query).GetEnumerator())
            {
                result = enumerator.MoveNext();

                if (result)
                    overlap = enumerator.Current;
            }

            return result;
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

        /// <inheritdoc/>
        public int MaximumDepth
        {
            get { return _root != null ? _root.Max : 0; }
        }

        /// <inheritdoc/>
        public bool AllowsReferenceDuplicates { get { return false; } }

        #endregion
    }
}
