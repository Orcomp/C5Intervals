using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

#if DEBUG
using QuickGraph;
using QuickGraph.Graphviz;
using QuickGraph.Graphviz.Dot;
#endif

namespace C5.Intervals
{
    /// <summary>
    /// An doubly-linked binary search tree for non-overlapping intervals.
    /// </summary>
    /// <typeparam name="I">The interval type.</typeparam>
    /// <typeparam name="T">The interval endpoint type.</typeparam>
    public class DoublyLinkedFiniteIntervalTree<I, T> : CollectionValueBase<I>, IIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Fields

        private Node _root;
        private readonly Node _first;
        private readonly Node _last;

        private int _count;

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        private void invariant()
        {
            // Check the balance invariant holds.
            Contract.Invariant(contractHelperConfirmBalance(_root));

            // First and last point to each other if the collection is empty
            Contract.Invariant(!IsEmpty || _first.Next == _last && _last.Previous == _first);
            // First and last are empty but the next and previous pointers respectively
            Contract.Invariant(_first.Key == null && _first.Previous == null && _first.Right == null && _first.Left == null && _first.Balance == 0);
            Contract.Invariant(_last.Key == null && _last.Next == null && _last.Right == null && _last.Left == null && _last.Balance == 0);
            Contract.Invariant(_first != _last);

            // Check enumerator is sorted
            Contract.Invariant(this.IsSorted<I, T>());

            // Check that doubly linked lists are sorted in both direction
            Contract.Invariant(nextNodes(_first).IsSorted());
            Contract.Invariant(previousNodes(_last.Previous).Select(n => n.Key).IsSorted(IntervalExtensions.CreateReversedComparer<I, T>()));

            // Check in-order traversal is sorted
            Contract.Invariant(contractHelperInOrderNodes(_root).IsSorted());
        }

        /// <summary>
        /// Checks that the contractHelperHeight of the tree is balanced.
        /// </summary>
        /// <returns>True if the tree is balanced, else false.</returns>
        [Pure]
        private static bool contractHelperConfirmBalance(Node root)
        {
            var result = true;
            contractHelperHeight(root, ref result);
            return result;
        }

        /// <summary>
        /// Get the contractHelperHeight of the tree.
        /// </summary>
        /// <param name="node">The node you wish to check the contractHelperHeight on.</param>
        /// <param name="result">Reference to a bool that will be set to false if an in-balance is discovered.</param>
        /// <returns>Height of the tree.</returns>
        [Pure]
        private static int contractHelperHeight(Node node, ref bool result)
        {
            if (node == null)
                return 0;

            var heightLeft = contractHelperHeight(node.Left, ref result);
            var heightRight = contractHelperHeight(node.Right, ref result);

            if (node.Balance != heightRight - heightLeft)
                result = false;

            return Math.Max(heightLeft, heightRight) + 1;
        }

        [Pure]
        private static IEnumerable<Node> contractHelperInOrderNodes(Node root)
        {
            if (root == null)
                yield break;

            foreach (var node in contractHelperInOrderNodes(root.Left))
                yield return node;

            yield return root;

            foreach (var node in contractHelperInOrderNodes(root.Right))
                yield return node;
        }

        #endregion

        #region Inner Classes

        private sealed class Node : IComparable<Node>
        {
            #region Fields

            public I Key;

            public Node Left;
            public Node Right;

            public Node Previous;
            public Node Next;

            public int Balance;

#if DEBUG
            public readonly bool Dummy;
#endif

            #endregion

            #region Code Contracts

            [ContractInvariantMethod]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
            private void invariant()
            {
            }

            #endregion

            #region Constructor

            public Node(I key, Node previous)
            {
                Contract.Requires(key != null);
                Contract.Requires(previous != null);
                Contract.Requires(previous.Next != null);
                Contract.Ensures(key != null);
                Contract.Ensures(Next != null && Previous != null);

                Key = key;
                insertAfter(previous);
            }

            public Node()
            {
#if DEBUG
                Dummy = true;
#endif
            }

            #endregion

            private void insertAfter(Node previous)
            {
                Contract.Requires(previous != null);
                Contract.Requires(previous.Next != null);
                Contract.Ensures(Contract.OldValue(previous.Next) == previous.Next.Next);
                Contract.Ensures(previous.Next == this);
                Contract.Ensures(this.Previous == previous);
                Contract.Ensures(Contract.OldValue(previous.Next).Previous == this);
                Contract.Ensures(this.Next == Contract.OldValue(previous.Next));

                var next = previous.Next;

                previous.Next = this;
                Previous = previous;

                Next = next;
                next.Previous = this;
            }

            public void Remove()
            {

                Previous.Next = Next;
                Next.Previous = Previous;
            }

            public int CompareTo(Node other)
            {
                return Key.CompareTo(other.Key);
            }

            public void Swap(Node successor)
            {
                Contract.Requires(successor != null);

                var tmp = Key;
                Key = successor.Key;
                successor.Key = tmp;
            }

            public override string ToString()
            {
                return Key == null ? "" : Key.ToIntervalString();
            }
        }

        #endregion

        #region AVL Tree Methods

        private static Node rotateForAdd(Node root, ref bool rotationNeeded)
        {
            Contract.Requires(root != null);

            Contract.Requires(root.Balance != -2 || root.Left != null);
            Contract.Requires(root.Balance != -2 || root.Left.Balance != -1 || root.Left.Left != null);
            Contract.Requires(root.Balance != -2 || root.Left.Balance != +1 || root.Left.Right != null);

            Contract.Requires(root.Balance != +2 || root.Right != null);
            Contract.Requires(root.Balance != +2 || root.Right.Balance != -1 || root.Right.Left != null);
            Contract.Requires(root.Balance != +2 || root.Right.Balance != +1 || root.Right.Right != null);

            switch (root.Balance)
            {
                // Node is balanced after the node was added
                case 0:
                    rotationNeeded = false;
                    break;

                // Node is unbalanced, so we rotate
                case -2:
                    switch (root.Left.Balance)
                    {
                        // Left Left Case
                        case -1:
                            root = rotateRight(root);
                            root.Balance = root.Right.Balance = 0;
                            break;

                        // Left Right Case
                        case +1:
                            root.Left = rotateLeft(root.Left);
                            root = rotateRight(root);

                            // root.Balance is either -1, 0, or +1
                            root.Left.Balance = (sbyte) (root.Balance == +1 ? -1 : 0);
                            root.Right.Balance = (sbyte) (root.Balance == -1 ? +1 : 0);
                            root.Balance = 0;
                            break;
                    }
                    rotationNeeded = false;
                    break;

                // Node is unbalanced, so we rotate
                case +2:
                    switch (root.Right.Balance)
                    {
                        // Right Right Case
                        case +1:
                            root = rotateLeft(root);
                            root.Balance = root.Left.Balance = 0;
                            break;

                        // Right Left Case
                        case -1:
                            root.Right = rotateRight(root.Right);
                            root = rotateLeft(root);

                            // root.Balance is either -1, 0, or +1
                            root.Left.Balance = (sbyte) (root.Balance == +1 ? -1 : 0);
                            root.Right.Balance = (sbyte) (root.Balance == -1 ? +1 : 0);
                            root.Balance = 0;
                            break;
                    }

                    rotationNeeded = false;
                    break;
            }

            return root;
        }

        private static Node rotateForRemove(Node root, ref bool rotationNeeded)
        {
            Contract.Requires(root != null);

            Contract.Requires(root.Balance != -2 || root.Left != null);
            Contract.Requires(root.Balance != -2 || root.Left.Balance != -1 || root.Left.Left != null);
            Contract.Requires(root.Balance != -2 || root.Left.Balance != +1 || root.Left.Right != null);

            Contract.Requires(root.Balance != +2 || root.Right != null);
            Contract.Requires(root.Balance != +2 || root.Right.Balance != -1 || root.Right.Left != null);
            Contract.Requires(root.Balance != +2 || root.Right.Balance != +1 || root.Right.Right != null);

            switch (root.Balance)
            {
                // High will not change for parent, so we can stop here
                case -1:
                case +1:
                    rotationNeeded = false;
                    break;

                // Node is unbalanced, so we rotate
                case -2:
                    switch (root.Left.Balance)
                    {
                        // Left Left Case
                        case -1:
                            root = rotateRight(root);
                            root.Balance = root.Right.Balance = 0;
                            break;

                        case 0:
                            root = rotateRight(root);
                            root.Right.Balance = -1;
                            root.Balance = +1;
                            rotationNeeded = false;
                            break;

                        // Left Right Case
                        case +1:
                            root.Left = rotateLeft(root.Left);
                            root = rotateRight(root);

                            // root.Balance is either -1, 0, or +1
                            root.Left.Balance = (sbyte) ((root.Balance == +1) ? -1 : 0);
                            root.Right.Balance = (sbyte) ((root.Balance == -1) ? +1 : 0);
                            root.Balance = 0;
                            break;
                    }
                    break;

                // Node is unbalanced, so we rotate
                case +2:
                    switch (root.Right.Balance)
                    {
                        // Right Right Case
                        case +1:
                            root = rotateLeft(root);
                            root.Balance = root.Left.Balance = 0;
                            break;

                        case 0:
                            root = rotateLeft(root);
                            root.Left.Balance = 1;
                            root.Balance = -1;
                            rotationNeeded = false;
                            break;

                        // Right Left Case
                        case -1:
                            root.Right = rotateRight(root.Right);
                            root = rotateLeft(root);

                            // root.Balance is either -1, 0, or +1
                            root.Left.Balance = (sbyte) (root.Balance == +1 ? -1 : 0);
                            root.Right.Balance = (sbyte) (root.Balance == -1 ? +1 : 0);
                            root.Balance = 0;
                            break;
                    }
                    break;
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

            return node;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create an empty Doubly-linked Finite Interval Tree.
        /// </summary>
        public DoublyLinkedFiniteIntervalTree()
        {
            Contract.Ensures(_first != null);
            Contract.Ensures(_last != null);
            Contract.Ensures(_first.Next == _last);
            Contract.Ensures(_last.Previous == _first);

            _first = new Node();
            _last = new Node();

            _first.Next = _last;
            _last.Previous = _first;
        }

        /// <summary>
        /// Create an Doubly-linked Finite Interval Tree from a collection of intervals.
        /// </summary>
        public DoublyLinkedFiniteIntervalTree(IEnumerable<I> intervals)
            : this()
        {
            Contract.Requires(intervals != null);

            // TODO: Remove ordering as soon as default behavior has been decided on
            foreach (var interval in intervals.OrderBy(x => x, IntervalExtensions.CreateComparer<I, T>()))
                Add(interval);
        }

        #endregion

        #region Collection Value

        /// <inheritdoc/>
        public override bool IsEmpty
        {
            get
            {
                Contract.Ensures(Contract.Result<bool>() == (_count == 0));
                Contract.Ensures(Contract.Result<bool>() == (_root == null));
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

            return _root.Key;
        }

        #endregion

        #region Events

        /// <inheritdoc/>
        public override EventTypeEnum ListenableEvents { get { return EventTypeEnum.Basic; } }
        //public EventTypeEnum ActiveEvents { get; private set; }
        //public event CollectionChangedHandler<T> CollectionChanged;
        //public event CollectionClearedHandler<T> CollectionCleared;
        //public event ItemsAddedHandler<T> ItemsAdded;
        //public event ItemInsertedHandler<T> ItemInserted;
        //public event ItemsRemovedHandler<T> ItemsRemoved;
        //public event ItemRemovedAtHandler<T> ItemRemovedAt;

        #endregion

        #region Interval Collection

        #region Properties

        #region Data Structure Properties

        /// <inheritdoc/>
        public bool AllowsOverlaps { get { return false; } }

        /// <inheritdoc/>
        public bool AllowsReferenceDuplicates { get { return false; } }

        #endregion

        #region Collection Properties

        /// <inheritdoc/>
        public IInterval<T> Span { get { return new IntervalBase<T>(_first.Next.Key, _last.Previous.Key); } }

        /// <inheritdoc/>
        public I LowestInterval { get { return _first.Next.Key; } }

        /// <inheritdoc/>
        public IEnumerable<I> LowestIntervals
        {
            get
            {
                if (IsEmpty)
                    yield break;

                yield return LowestInterval;
            }
        }

        /// <inheritdoc/>
        public I HighestInterval { get { return _last.Previous.Key; } }

        /// <inheritdoc/>
        public IEnumerable<I> HighestIntervals
        {
            get
            {
                if (IsEmpty)
                    yield break;

                yield return HighestInterval;
            }
        }

        /// <inheritdoc/>
        public int MaximumDepth { get { return IsEmpty ? 0 : 1; } }

        #endregion

        #endregion

        #region Enumerable

        /// <inheritdoc/>
        public override IEnumerator<I> GetEnumerator() { return Sorted.GetEnumerator(); }

        /// <inheritdoc/>
        public IEnumerable<I> Sorted { get { return nextIntervals(_first.Next); } }

        /// <summary>
        /// Get the intervals in reverse order sorted in descending endpoint order.
        /// </summary>
        [Pure]
        public IEnumerable<I> Reverse
        {
            get { return previousNodes(_last.Previous).Select(node => node.Key); }
        }

        [Pure]
        private IEnumerable<Node> nextNodes(Node node)
        {
            Contract.Requires(node != null);

            // Skip the _first node
            if (node == _first)
                node = _first.Next;

            // Iterate until the _last node
            while (node != _last)
            {
                yield return node;
                node = node.Next;
            }
        }

        [Pure]
        private IEnumerable<I> nextIntervals(Node node)
        {
            return nextNodes(node).Select(n => n.Key);
        }

        [Pure]
        private IEnumerable<Node> previousNodes(Node node)
        {
            Contract.Requires(node != null);

            // Skip the _last node
            if (node == _last)
                node = _last.Previous;

            // Iterate until the _first node
            while (node != _first)
            {
                yield return node;
                node = node.Previous;
            }
        }

        #endregion

        #region Find Overlaps

        /// <inheritdoc/>
        public IEnumerable<I> FindOverlaps(T query)
        {
            I overlap;
            if (FindOverlap(query, out overlap))
                yield return overlap;
        }

        /// <inheritdoc/>
        public IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            if (IsEmpty)
                yield break;

            var node = _root;

            // Search for the node containing the first overlap
            while (true)
            {
                var compare = query.CompareLowHigh(node.Key);

                if (compare < 0)
                {
                    if (node.Left == null)
                        break;

                    // Move left
                    node = node.Left;
                }
                else if (compare > 0)
                {
                    if (node.Right == null)
                    {
                        // If there is no right node, use the next in sorting order
                        node = node.Next;

                        // Make sure the low is still lower or equal to the query's high
                        if (node == _last || query.CompareLowHigh(node.Key) > 0)
                            yield break;

                        break;
                    }

                    // Move right
                    node = node.Right;
                }
                else
                    break;
            }

            // Iterate overlaps
            foreach (var interval in nextIntervals(node).TakeWhile(x => x.CompareLowHigh(query) <= 0))
                yield return interval;
        }

        #endregion

        #region Find Overlap

        /// <inheritdoc/>
        public bool FindOverlap(T query, out I overlap)
        {
            overlap = null;

            // Stop immediately if empty
            if (IsEmpty)
                return false;

            var node = _root;
            while (true)
            {
                var compare = query.CompareTo(node.Key.Low);

                if (compare < 0)
                {
                    // Continue search left if possible
                    if (node.Left != null)
                        node = node.Left;
                    else
                    {
                        // Move to the previous node as it might contain an overlapping interval
                        node = node.Previous;

                        // Check if previous interval overlaps
                        if (node != _first && node.Key.Overlaps(query))
                        {
                            overlap = node.Key;
                            return true;
                        }

                        // Stop as no overlap exists
                        return false;
                    }
                }
                else if (compare > 0)
                {
                    // Continue search right if possible
                    if (node.Right != null)
                        node = node.Right;
                    else
                    {
                        // Check if interval overlaps
                        if (node.Key.Overlaps(query))
                        {
                            overlap = node.Key;
                            return true;
                        }

                        // Stop as no overlap exists
                        return false;
                    }
                }
                else
                {
                    // Check if low is included, thereby overlapping the query
                    if (node.Key.LowIncluded)
                    {
                        overlap = node.Key;
                        return true;
                    }

                    // Go to the previous node
                    node = node.Previous;

                    // Check if the previous interval overlaps
                    if (node != _first && node.Key.High.CompareTo(query) == 0 && node.Key.HighIncluded)
                    {
                        overlap = node.Key;
                        return true;
                    }

                    // Stop as no overlap exists
                    return false;
                }
            }
        }

        /// <inheritdoc/>
        public bool FindOverlap(IInterval<T> query, out I overlap)
        {
            overlap = null;

            var node = _root;

            while (node != null)
            {
                if (query.CompareHighLow(node.Key) < 0)
                    node = node.Left;
                else if (query.CompareLowHigh(node.Key) > 0)
                    node = node.Right;
                else
                {
                    overlap = node.Key;
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Count Overlaps

        public int CountOverlaps(T query)
        {
            I overlap;
            return FindOverlap(query, out overlap) ? 1 : 0;
        }

        public int CountOverlaps(IInterval<T> query)
        {
            // TODO: Implement with count variable in each node

            return FindOverlaps(query).Count();
        }

        #endregion

        #region Gaps

        /// <inheritdoc/>
        public IEnumerable<IInterval<T>> Gaps { get { return Sorted.Gaps(); } }

        /// <inheritdoc/>
        public IEnumerable<IInterval<T>> FindGaps(IInterval<T> query) { return FindOverlaps(query).Gaps(query); }

        #endregion

        #region Extensible

        /// <inheritdoc/>
        public bool IsReadOnly { get { return false; } }

        #region Add

        /// <inheritdoc/>
        public bool Add(I interval)
        {
            Contract.Ensures(Contract.Result<bool>() != Contract.OldValue(Contract.Exists(this, x => x.Overlaps(interval))));

            var intervalWasAdded = false;
            var rotationNeeded = false;

            _root = add(interval, _root, _first, ref rotationNeeded, ref intervalWasAdded);

            if (intervalWasAdded)
            {
                _count++;
                raiseForAdd(interval);
            }

            return intervalWasAdded;
        }

        /// <inheritdoc/>
        public void AddAll(IEnumerable<I> intervals)
        {
            foreach (var interval in intervals)
                Add(interval);
        }

        private Node add(I interval, Node root, Node previous, ref bool rotationNeeded, ref bool intervalWasAdded)
        {
            if (root == null)
            {
                // Return if previous or next overlaps interval
                if (previous != _first && previous.Key.CompareHighLow(interval) >= 0 ||
                    previous.Next != _last && interval.CompareHighLow(previous.Next.Key) >= 0)
                    return null;

                rotationNeeded = true;
                intervalWasAdded = true;
                return new Node(interval, previous);
            }

            var compare = interval.CompareLow(root.Key);

            if (compare < 0)
            {
                root.Left = add(interval, root.Left, root.Previous, ref rotationNeeded, ref intervalWasAdded);

                // Adjust node balance, if node was added
                if (rotationNeeded)
                    root.Balance--;
            }
            else if (compare > 0)
            {
                root.Right = add(interval, root.Right, root, ref rotationNeeded, ref intervalWasAdded);

                // Adjust node balance, if node was added
                if (rotationNeeded)
                    root.Balance++;
            }
            else
            {
                // Interval was not added as it was contained already
                return root;
            }

            // Tree might be unbalanced after node was added, so we rotate
            if (rotationNeeded)
                root = rotateForAdd(root, ref rotationNeeded);

            return root;
        }

        public bool ForceAdd(I interval, Func<I, I, bool> action, bool continueWhenNoConflict = false, bool forcePosition = false)
        {
            return ForceAdd(interval, action, () => continueWhenNoConflict, forcePosition);
        }

        /// <summary>
        /// Forcingly adds an interval, even if it has overlaps in the collection. The function must resolve any overlapping conflict for a pair of overlapping intervals, by moving either interval
        ///  will be called to resolve any overlap conflicts arose from the insertion
        /// .
        /// </summary>
        /// <param name="interval">The interval.</param>
        /// <param name="action">The action, that will resolve overlap conflict.</param>
        /// <returns>True, if the action was called to resolve a conflict.</returns>
        public bool ForceAdd(I interval, Func<I, I, bool> action, Func<bool> continueWhenNoConflict, bool forcePosition = false)
        {
            var rotationNeeded = false;
            Node node;
            _root = forceAdd(interval, _root, _first, ref rotationNeeded, out node);
            _count++;

            // If interval has the same low value as node then we need to swap
            if (node.Next.Key != null)
            {
                if (node.Key.CompareLow(node.Next.Key) == 0)
                {
                    node.Next.Swap(node);
                }
            }

            if (forcePosition && node.Previous != _first && node.Previous.Key.CompareHighLow(node.Key) <= 0)
            {
                node.Previous.Swap(node);
                node = node.Previous;
            }

            var result = false;

            while (node.Next != _last && node.Key.CompareHighLow(node.Next.Key) >= 0 || continueWhenNoConflict())
            {
                result = true;
                if (action(node.Key, node.Next.Key))
                {
                    Remove(node.Next.Key);
                    continue;
                }

                if (node.Key.CompareHighLow(node.Next.Key) >= 0)
                    throw new InvalidOperationException("The interval has to be schedule after the previous one. The invariant is now broken.");

                // Move to next node
                node = node.Next;
            }

            return result;
        }

        private Node forceAdd(I interval, Node root, Node previous, ref bool rotationNeeded, out Node startNode)
        {
            if (root == null)
            {
                rotationNeeded = true;
                var node = new Node(interval, previous);

                startNode = previous != _first && previous.Key.CompareHighLow(interval) >= 0 ? previous : node;

                return node;
            }

            var compare = interval.CompareLow(root.Key);

            if (compare < 0)
            {
                root.Left = forceAdd(interval, root.Left, root.Previous, ref rotationNeeded, out startNode);

                // Adjust node balance, if node was added
                if (rotationNeeded)
                    root.Balance--;
            }
            else
            {
                root.Right = forceAdd(interval, root.Right, root, ref rotationNeeded, out startNode);

                // Adjust node balance, if node was added
                if (rotationNeeded)
                    root.Balance++;
            }

            // Tree might be unbalanced after node was added, so we rotate
            if (rotationNeeded)
                root = rotateForAdd(root, ref rotationNeeded);

            return root;
        }

        #endregion

        #region Remove

        /// <inheritdoc/>
        public bool Remove(I interval)
        {
            var intervalWasRemoved = false;
            var rotationNeeded = false;
            _root = remove(interval, _root, ref intervalWasRemoved, ref rotationNeeded);

            if (intervalWasRemoved)
            {
                _count--;
                raiseForRemove(interval);
            }

            return intervalWasRemoved;
        }

        private static Node remove(I interval, Node root, ref bool intervalWasRemoved, ref bool rotationNeeded)
        {
            if (root == null)
                return null;

            var compare = interval.CompareTo(root.Key);

            if (compare < 0)
            {
                root.Left = remove(interval, root.Left, ref intervalWasRemoved, ref rotationNeeded);

                if (rotationNeeded)
                    root.Balance++;
            }
            else if (compare > 0)
            {
                root.Right = remove(interval, root.Right, ref intervalWasRemoved, ref rotationNeeded);

                if (rotationNeeded)
                    root.Balance--;
            }
            else if (!ReferenceEquals(root.Key, interval))
                return root;
            else if (root.Left != null && root.Right != null)
            {
                var successor = root.Next;

                // Swap root and successor nodes
                root.Swap(successor);

                // Remove the successor node
                root.Right = remove(successor.Key, root.Right, ref intervalWasRemoved, ref rotationNeeded);

                if (rotationNeeded)
                    root.Balance--;

                Contract.Assert(intervalWasRemoved);
            }
            else
            {
                rotationNeeded = true;
                intervalWasRemoved = true;
                root.Remove();

                // Return Left if not null, otherwise Right
                return root.Left ?? root.Right;
            }


            if (rotationNeeded)
                root = rotateForRemove(root, ref rotationNeeded);

            return root;
        }

        #endregion

        #region Clear

        /// <inheritdoc/>
        public void Clear()
        {
            // Return if tree is empty
            if (IsEmpty)
                return;

            // Save old count and reset all values
            var oldCount = _count;
            clear();

            // Raise events
            if ((ActiveEvents & EventTypeEnum.Cleared) != 0)
                raiseCollectionCleared(true, oldCount);
            if ((ActiveEvents & EventTypeEnum.Changed) != 0)
                raiseCollectionChanged();
        }

        private void clear()
        {
            _root = null;

            _first.Next = _last;
            _last.Previous = _first;

            _count = 0;
        }

        #endregion

        #endregion

        #endregion

        #region QuickGraph

#if DEBUG
        /// <summary>
        /// Get a string representation of the tree in GraphViz dot format using QuickGraph.
        /// </summary>
        /// <returns>GraphViz string.</returns>
        public string QuickGraph
        {
            get
            {
                var graph = new AdjacencyGraph<Node, Edge<Node>>();
                if (_root != null)
                {
                    var node = new Node();
                    graph.AddVertex(node);
                    graph.AddEdge(new Edge<Node>(node, _root));
                }

                foreach (var node in contractHelperInOrderNodes(_root))
                {
                    graph.AddVertex(node);
                    if (node.Left != null)
                    {
                        graph.AddVertex(node.Left);
                        graph.AddEdge(new Edge<Node>(node, node.Left));
                    }
                    else
                    {
                        var dummy = new Node();
                        graph.AddVertex(dummy);
                        graph.AddEdge(new Edge<Node>(node, dummy));
                    }

                    if (node.Right != null)
                    {
                        graph.AddVertex(node.Right);
                        graph.AddEdge(new Edge<Node>(node, node.Right));
                    }
                    else
                    {
                        var dummy = new Node();
                        graph.AddVertex(dummy);
                        graph.AddEdge(new Edge<Node>(node, dummy));
                    }

                    /* 
                    if (node.Previous != null)
                    {
                        graph.AddVertex(node.Previous);
                        graph.AddEdge(new Edge<Node>(node, node.Previous));
                    }
                    else
                    {
                        var dummy = new Node();
                        graph.AddVertex(dummy);
                        graph.AddEdge(new Edge<Node>(node, dummy));
                    }

                    if (node.Next != null)
                    {
                        graph.AddVertex(node.Next);
                        graph.AddEdge(new Edge<Node>(node, node.Next));
                    }
                    else
                    {
                        var dummy = new Node();
                        graph.AddVertex(dummy);
                        graph.AddEdge(new Edge<Node>(node, dummy));
                    }
                    */
                }

                var gw = new GraphvizAlgorithm<Node, Edge<Node>>(graph);
                gw.FormatVertex += delegate(object sender, FormatVertexEventArgs<Node> e)
                {
                    if (e.Vertex.Dummy)
                    {
                        e.VertexFormatter.Shape = GraphvizVertexShape.Point;
                    }
                    else
                    {
                        e.VertexFormatter.Shape = GraphvizVertexShape.Record;
                        e.VertexFormatter.Style = GraphvizVertexStyle.Rounded;
                        e.VertexFormatter.Font = new GraphvizFont("consola", 12);
                        // Generate main cell
                        var cell = new GraphvizRecordCell();
                        // Add Key in top cell
                        cell.Cells.Add(new GraphvizRecordCell { Text = e.Vertex.Key.ToString() });


                        // Add cell to record
                        e.VertexFormatter.Record.Cells.Add(cell);
                    }
                };
                return gw.Generate();
            }
        }

#endif

        #endregion
    }
}
