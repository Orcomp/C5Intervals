using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.Contracts;
using System.Text;

namespace C5.Intervals
{
    /// <summary>
    /// An implementation of the Dynamic Interval Tree.
    /// </summary>
    /// <remarks>It is important to implement a proper hashing method for <see cref="I"/> not based on endpoints. Failing to do so will result in worse runtimes for interval duplicate objects.</remarks>
    /// <typeparam name="I">The interval type.</typeparam>
    /// <typeparam name="T">The interval endpoint type.</typeparam>
    public class DynamicIntervalTree<I, T> : CollectionValueBase<I>, IIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Fields

        private Node _root;
        private int _count;

        // Comparer for IntervalList (sorting on high in non-ascending order)
        private static readonly IComparer<IInterval<T>> Comparer = ComparerFactory<IInterval<T>>.CreateComparer((x, y) => y.CompareHigh(x));
        // Comparer for sublists in IntervalList
        private static readonly IEqualityComparer<IInterval<T>> EqualityComparer =
            ComparerFactory<IInterval<T>>.CreateEqualityComparer(ReferenceEquals, x => x.GetHashCode());

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        private void invariant()
        {
            // Check the balance invariant holds
            Contract.Invariant(contractHelperConfirmBalance(_root));

            // Check node spans
            Contract.Invariant(contractHelperCheckNodeSpans(_root));

            Contract.Invariant(IsEmpty && _root == null || !_root.Span.IsEmpty);

            // The left most node will always contain at least one interval
            Contract.Invariant(IsEmpty || contractHelperLowestNodeIsNonEmpty(_root));

            // Check nodes are sorted
            Contract.Invariant(nodes(_root).IsSorted());

            // Check that the maximum depth variables are correct for all nodes
            Contract.Invariant(contractHelperCheckMnoForEachNode(_root));
        }

        [Pure]
        private static bool contractHelperLowestNodeIsNonEmpty(Node root)
        {
            Contract.Requires(root != null);

            while (root.Left != null)
                root = root.Left;

            return root.IncludedList != null || root.ExcludedList != null;
        }

        [Pure]
        private static bool contractHelperCheckNodeSpans(Node root)
        {
            foreach (var node in nodes(root))
            {
                // There is a span
                if (!node.Span.IsEmpty)
                {
                    // Check that span contains local span
                    if (!node.LocalSpan.IsEmpty && !node.Span.Contains(node.LocalSpan))
                        return false;

                    // If span is set then left, right and local span cannot be null at the same time
                    if (node.LocalSpan.IsEmpty && (node.Left == null || node.Left.Span.IsEmpty) && (node.Right == null || node.Right.Span.IsEmpty))
                        return false;

                    // Span must contain left's span
                    if (node.Left != null && !node.Left.Span.IsEmpty && !node.Span.Contains(node.Left.Span))
                        return false;
                    // Span must contain right's span
                    if (node.Right != null && !node.Right.Span.IsEmpty && !node.Span.Contains(node.Right.Span))
                        return false;
                }
                // If span is null, then local span and subtree spans must be null too
                else
                {
                    if (!node.LocalSpan.IsEmpty || (node.Left != null && !node.Left.Span.IsEmpty) || (node.Right != null && !node.Right.Span.IsEmpty))
                        return false;
                }

            }

            return true;
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
        private bool contractHelperCheckMnoForEachNode(Node root)
        {
            if (root != null && root.Sum != 0)
                return false;

            foreach (var keyValuePair in contractHelperGetIntervalsByEndpoint())
            {
                var key = keyValuePair.Key;
                var intervals = keyValuePair.Value;

                var deltaAt = 0;
                var deltaAfter = 0;

                foreach (var interval in intervals)
                {
                    if (interval.Low.CompareTo(key) == 0)
                    {
                        if (interval.LowIncluded)
                            deltaAt++;
                        else
                            deltaAfter++;
                    }

                    if (interval.High.CompareTo(key) == 0)
                    {
                        if (interval.HighIncluded)
                            deltaAfter--;
                        else
                            deltaAt--;
                    }
                }

                var node = contractHelperFindNode(root, key);

                // Check DeltaAt and DeltaAfter
                if (node.DeltaAt != deltaAt || node.DeltaAfter != deltaAfter)
                    return false;

                // Check Sum and Max
                if (!contractHelperCheckMno(node))
                    return false;
            }

            return true;
        }

        [Pure]
        private IEnumerable<KeyValuePair<T, ArrayList<I>>> contractHelperGetIntervalsByEndpoint()
        {
            var dictionary = new TreeDictionary<T, ArrayList<I>>();

            foreach (var interval in this)
            {
                // Make sure the sets exist
                if (!dictionary.Contains(interval.Low))
                    dictionary.Add(interval.Low, new ArrayList<I>());
                if (!dictionary.Contains(interval.High))
                    dictionary.Add(interval.High, new ArrayList<I>());

                // Add interval for low and high
                dictionary[interval.Low].Add(interval);

                if (!interval.IsPoint())
                    dictionary[interval.High].Add(interval);
            }

            return dictionary;
        }

        [Pure]
        private static bool contractHelperCheckMno(Node node)
        {
            Contract.Requires(node != null);

            // Check sum
            var sum = (node.Left != null ? node.Left.Sum : 0) + node.DeltaAt + node.DeltaAfter +
                      (node.Right != null ? node.Right.Sum : 0);

            if (node.Sum != sum)
                return false;

            // Check max
            var max = 0;

            sum = node.Left != null ? node.Left.Max : 0;
            if (sum > max)
                max = sum;

            sum = (node.Left != null ? node.Left.Sum : 0) + node.DeltaAt;
            if (sum > max)
                max = sum;

            sum = (node.Left != null ? node.Left.Sum : 0) + node.DeltaAt + node.DeltaAfter;
            if (sum > max)
                max = sum;

            sum = (node.Left != null ? node.Left.Sum : 0) + node.DeltaAt + node.DeltaAfter + (node.Right != null ? node.Right.Max : 0);
            if (sum > max)
                max = sum;

            if (node.Max != max)
                return false;

            return true;
        }

        /// <summary>
        /// Find the node containing the search key.
        /// </summary>
        /// <param name="node">The root node which subtree should be searched.</param>
        /// <param name="key">The key being searched.</param>
        /// <returns>The node containing the key if it exists, otherwise null.</returns>
        [Pure]
        private static Node contractHelperFindNode(Node node, T key)
        {
            Contract.Requires(node != null);
            Contract.Ensures(Contract.Result<Node>() != null);

            while (node != null)
            {
                var compare = key.CompareTo(node.Key);

                if (compare > 0)
                    node = node.Right;
                else if (compare < 0)
                    node = node.Left;
                else
                    break;
            }

            return node;
        }

        #endregion

        #region Inner Classes

        private class SpanInterval : IInterval<T>
        {
            #region Fields

            public bool IsEmpty { get; set; }

            public T Low { get; private set; }
            public T High { get; private set; }
            public bool LowIncluded { get; private set; }
            public bool HighIncluded { get; private set; }

            #endregion

            #region Constructor

            public SpanInterval(IInterval<T> interval = null)
            {
                // Use a null interval to create an empty interval
                if (interval == null)
                    IsEmpty = true;
                else
                {
                    IsEmpty = false;

                    Low = interval.Low;
                    High = interval.High;
                    LowIncluded = interval.LowIncluded;
                    HighIncluded = interval.HighIncluded;
                }
            }

            #endregion

            #region Set Endpoints

            public void SetInterval(SpanInterval interval)
            {
                if (interval.IsEmpty)
                    IsEmpty = true;
                else
                {
                    IsEmpty = false;

                    Low = interval.Low;
                    LowIncluded = interval.LowIncluded;
                    High = interval.High;
                    HighIncluded = interval.HighIncluded;
                }
            }

            public void SetInterval(IInterval<T> interval)
            {
                //Contract.Requires(interval.IsValidInterval());

                IsEmpty = false;

                Low = interval.Low;
                LowIncluded = interval.LowIncluded;
                High = interval.High;
                HighIncluded = interval.HighIncluded;
            }

            public void SetInterval(IInterval<T> low, IInterval<T> high)
            {
                //Contract.Requires(low != null);
                //Contract.Requires(high != null);
                //Contract.Requires(low.Low.CompareTo(high.High) < 0 || low.Low.CompareTo(high.High) == 0 && low.LowIncluded && high.HighIncluded);

                IsEmpty = false;

                Low = low.Low;
                LowIncluded = low.LowIncluded;
                High = high.High;
                HighIncluded = high.HighIncluded;
            }

            public void ExpandInterval(IInterval<T> interval)
            {
                //Contract.Requires(!IsEmpty);
                //Contract.Requires(interval.IsValidInterval());

                if (interval.CompareLow(this) < 0)
                {
                    Low = interval.Low;
                    LowIncluded = interval.LowIncluded;
                }
                if (this.CompareHigh(interval) < 0)
                {
                    High = interval.High;
                    HighIncluded = interval.HighIncluded;
                }
            }

            #endregion
        }

        private class IntervalList : IEnumerable<I>
        {
            // A dictionary sorted on high with an accompanying sublist to handle objects with duplicate intervals
            private readonly ISortedDictionary<I, HashBag<I>> _dictionary;

            #region Code Contracts

            [ContractInvariantMethod]
            private void invariant()
            {
                // Dictionary should never be null
                Contract.Invariant(_dictionary != null);

                // Confirm high placement
                Contract.Invariant(Contract.ForAll(_dictionary, pair => pair.Value == null || Contract.ForAll(pair.Value, x => pair.Key.CompareTo(x) == 0)));
            }

            [Pure]
            private int count
            {
                get
                {
                    Contract.Ensures(Contract.Result<int>() == Enumerable.Count(this));
                    Contract.Ensures(Contract.Result<int>() >= 0);
                    Contract.Ensures(IsEmpty || Contract.Result<int>() > 0);
                    return _dictionary.Sum(keyValuePair => 1 + (keyValuePair.Value == null ? 0 : keyValuePair.Value.Count));
                }
            }

            #endregion

            /// <summary>
            /// Create a new empty IntervalList.
            /// </summary>
            public IntervalList()
            {
                Contract.Ensures(_dictionary != null);
                Contract.Ensures(_dictionary.IsEmpty);

                // TODO: Benchmark against SortedArrayDictionary
                _dictionary = new TreeDictionary<I, HashBag<I>>(Comparer);
            }

            public bool Add(I interval)
            {
                Contract.Requires(interval != null);

                // The dictionary contains a key equal to the interval
                Contract.Ensures(Enumerable.Contains(_dictionary.Keys, interval));
                // The dictionary contains the interval
                Contract.Ensures(Contract.Exists(_dictionary, keyValuePair => ReferenceEquals(keyValuePair.Key, interval) || keyValuePair.Value != null && Contract.Exists(keyValuePair.Value, x => ReferenceEquals(x, interval))));
                // If the interval is added the count goes up by one
                Contract.Ensures(!Contract.Result<bool>() || count == Contract.OldValue(count) + 1);
                // If the interval is not added the count stays the same
                Contract.Ensures(Contract.Result<bool>() || count == Contract.OldValue(count));
                // If the list didn't contain an interval with the same high, then the sublist is null
                Contract.Ensures(Contract.OldValue(Enumerable.Contains(_dictionary.Keys, interval)) || _dictionary[interval] == null);

                var key = interval;
                HashBag<I> list;
                if (_dictionary.Find(ref key, out list))
                {
                    if (list == null)
                    {
                        _dictionary[key] = new HashBag<I>(EqualityComparer) { interval };
                        return true;
                    }

                    return list.Add(interval);
                }

                // We add a null sublist as we only have the one interval
                _dictionary.Add(interval, null);
                return true;
            }

            public bool Remove(I interval)
            {
                Contract.Requires(interval != null);

                // If the interval is removed the count goes down by one
                Contract.Ensures(!Contract.Result<bool>() || count == Contract.OldValue(count) - 1);
                // If the interval isn't removed the count stays the same
                Contract.Ensures(Contract.Result<bool>() || count == Contract.OldValue(count));
                // The result is true if the collection contained the interval before remove was called
                Contract.Ensures(Contract.Result<bool>() == Contract.OldValue(Contract.Exists(_dictionary, keyValuePair => ReferenceEquals(keyValuePair.Key, interval) || keyValuePair.Value != null && Contract.Exists(keyValuePair.Value, x => ReferenceEquals(x, interval)))));
                // Check that it was the correct interval that was removed
                Contract.Ensures(!Contract.Result<bool>() || this.Count(x => ReferenceEquals(x, interval)) == Contract.OldValue(this.Count(x => ReferenceEquals(x, interval))) - 1);
                Contract.Ensures(Contract.Result<bool>() || this.Count(x => ReferenceEquals(x, interval)) == Contract.OldValue(this.Count(x => ReferenceEquals(x, interval))));

                // Check if the list contains the interval
                if (!_dictionary.Contains(interval))
                    return false;

                var key = interval;
                HashBag<I> list;

                if (!_dictionary.Find(ref key, out list))
                    return false;

                // Check if the interval is used as the key
                if (ReferenceEquals(key, interval))
                {
                    // The list is empty
                    if (list == null || list.IsEmpty)
                        return _dictionary.Remove(interval);

                    var newKey = list.First();
                    list.Remove(newKey);
                    _dictionary.Remove(interval);
                    _dictionary.Add(newKey, (list.IsEmpty ? null : list));
                    return true;
                }

                // The list doesn't contain interval
                if (list == null)
                    return false;

                // Otherwise just remove it from the list
                return list.Remove(interval);
            }

            public bool IsEmpty
            {
                get
                {
                    Contract.Ensures(Contract.Result<bool>() == _dictionary.IsEmpty);
                    return _dictionary.IsEmpty;
                }
            }

            public I Choose()
            {
                Contract.EnsuresOnThrow<NoSuchItemException>(IsEmpty);
                Contract.Ensures(IsEmpty || Contract.Result<I>() != null && Contract.Exists(this, x => ReferenceEquals(x, Contract.Result<I>())));
                if (IsEmpty)
                    throw new NoSuchItemException();

                return _dictionary.First().Key;
            }

            [Pure]
            public I Highest
            {
                get
                {
                    Contract.Requires(!IsEmpty);
                    Contract.Ensures(Contract.ForAll(this, x => x.CompareHigh(Contract.Result<I>()) <= 0));
                    Contract.Ensures(Contract.Exists(this, x => ReferenceEquals(x, Contract.Result<I>())));

                    return _dictionary.First().Key;
                }
            }

            public IEnumerable<I> FindOverlaps(IInterval<T> query)
            {
                // Stabbing value cannot be null
                Contract.Requires(query != null);

                // The collection of intervals that overlap the query must be equal to the result
                Contract.Ensures(IntervalCollectionContractHelper.CollectionEquals(this.Where(x => x.Overlaps(query)), Contract.Result<IEnumerable<I>>()));

                // All intervals in the collection that do not overlap cannot by in the result
                Contract.Ensures(Contract.ForAll(this.Where(x => !x.Overlaps(query)), x => Contract.ForAll(Contract.Result<IEnumerable<I>>(), y => !ReferenceEquals(x, y))));

                foreach (var keyValuePair in _dictionary.TakeWhile(keyValuePair => keyValuePair.Key.Overlaps(query)))
                {
                    // Return indexed interval
                    yield return keyValuePair.Key;

                    // Return intervals with duplicate endpoints
                    if (keyValuePair.Value != null)
                        foreach (var interval in keyValuePair.Value)
                            yield return interval;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<I> GetEnumerator()
            {
                foreach (var keyValuePair in _dictionary.RangeAll().Backwards())
                {
                    yield return keyValuePair.Key;

                    if (keyValuePair.Value != null)
                        foreach (var interval in keyValuePair.Value)
                            yield return interval;
                }
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                foreach (var list in this)
                    sb.Append(list);
                return sb.ToString();
            }

            public bool ContainsReferenceEqualInterval(I interval)
            {
                Contract.Requires(interval != null);
                // Check that there is a reference equal interval in the collection
                Contract.Ensures(Contract.Result<bool>() == Contract.Exists(_dictionary, keyValuePair => ReferenceEquals(keyValuePair.Key, interval) || keyValuePair.Value != null && Contract.Exists(keyValuePair.Value, x => ReferenceEquals(x, interval))));

                var key = interval;
                HashBag<I> list;
                if (_dictionary.Find(ref key, out list))
                    return ReferenceEquals(key, interval) || list != null && list.Any(i => ReferenceEquals(i, interval));

                return false;
            }
        }

        private sealed class Node : IComparable<Node>
        {
            #region Fields

            private bool _delete;

            // The interval which Low is used as key in the binary search tree
            public T Key;

            // Children
            public Node Left;
            public Node Right;

            // The span of the intervals in the successor
            public readonly SpanInterval LocalSpan;
            // The span of the subtree rooted in this successor
            public readonly SpanInterval Span;

            // List of intervals starting at the same low as Key
            public IntervalList IncludedList;
            public IntervalList ExcludedList;

            // Fields for Maximum Depth
            internal int DeltaAt;
            internal int DeltaAfter;
            public int Sum;
            public int Max;

            // AVL Balance
            public int Balance;

            #endregion

            #region Code Contracts

            [ContractInvariantMethod]
            private void invariant()
            {
                Contract.Invariant(Key != null);

                // The interval lists are either null or non-empty
                Contract.Invariant(IncludedList == null || !IncludedList.IsEmpty);
                Contract.Invariant(ExcludedList == null || !ExcludedList.IsEmpty);

                // If there is a local span then both lists can't be empty
                Contract.Invariant(LocalSpan.IsEmpty || !(IncludedList == null && ExcludedList == null));

                // Local span contains all intervals
                Contract.Invariant(LocalSpan.IsEmpty || IncludedList == null || Contract.ForAll(IncludedList, i => LocalSpan.Contains(i)));
                Contract.Invariant(LocalSpan.IsEmpty || ExcludedList == null || Contract.ForAll(ExcludedList, i => LocalSpan.Contains(i)));

                // Local span has the lowest low in the lists
                Contract.Invariant(LocalSpan.IsEmpty || !LocalSpan.LowIncluded || IncludedList != null);
                Contract.Invariant(LocalSpan.IsEmpty || LocalSpan.LowIncluded || ExcludedList != null);

                // Local span has the highest high in the lists
                Contract.Invariant(LocalSpan.IsEmpty ||
                    IncludedList != null && ExcludedList != null && IncludedList.Highest.HighestHigh(ExcludedList.Highest).CompareHigh(LocalSpan) == 0 ||
                    IncludedList == null && ExcludedList.Highest.CompareHigh(LocalSpan) == 0 ||
                    ExcludedList == null && IncludedList.Highest.CompareHigh(LocalSpan) == 0);

                // Check interval placement on low
                Contract.Invariant(IncludedList == null || Contract.ForAll(IncludedList, x => x.Low.Equals(Key) && x.LowIncluded));
                Contract.Invariant(ExcludedList == null || Contract.ForAll(ExcludedList, x => x.Low.Equals(Key) && !x.LowIncluded));
            }

            #endregion

            #region Constructor

            public Node(T key, bool highIncluded)
            {
                Key = key;

                // Set spans
                LocalSpan = new SpanInterval();
                Span = new SpanInterval();

                AddHighToDelta(highIncluded);

                // Set sum
                Sum = -1;
            }

            public Node(I interval)
            {
                Contract.Requires(interval != null);
                Contract.Ensures(Key != null);
                Contract.Ensures(!LocalSpan.IsEmpty);
                Contract.Ensures(!Span.IsEmpty);
                Contract.Ensures(Key.Equals(interval.Low));
                Contract.Ensures(LocalSpan.IntervalEquals(interval));
                Contract.Ensures(Span.IntervalEquals(interval));
                Contract.Ensures(interval.LowIncluded && IncludedList != null || ExcludedList != null);

                Key = interval.Low;

                // Set spans
                LocalSpan = new SpanInterval(interval);
                Span = new SpanInterval(interval);

                // Insert the interval into a list
                AddIntervalToList(interval);

                // Max and sum
                Max = Sum = 1;
            }

            #endregion

            #region Methods

            public bool IsEmpty
            {
                get
                {
                    return _delete || IncludedList == null && ExcludedList == null && DeltaAt == 0 && DeltaAfter == 0;
                }

                private set { _delete = value; }
            }

            public bool UpdateSpan()
            {
                // No children
                if (Left == null && Right == null)
                    Span.SetInterval(LocalSpan);
                // No right child with a span
                else if (Right == null || Right.Span.IsEmpty)
                {
                    // Left node is an endpoint node
                    if (Left == null || Left.Span.IsEmpty)
                        Span.SetInterval(LocalSpan);
                    // This node is an endpoint node
                    else if (LocalSpan.IsEmpty)
                        Span.SetInterval(Left.Span);
                    // Neither is an endpoint node
                    else
                    {
                        if (Left.Span.CompareHigh(LocalSpan) >= 0)
                            Span.SetInterval(Left.Span);
                        else
                            Span.SetInterval(Left.Span, LocalSpan);
                    }
                }
                // No left child with a span
                else if (Left == null || Left.Span.IsEmpty)
                {
                    // This node is an endpoint node
                    if (LocalSpan.IsEmpty)
                        Span.SetInterval(Right.Span);
                    // Neither is an endpoint node
                    else
                    {
                        if (Right.Span.CompareHigh(LocalSpan) > 0)
                            Span.SetInterval(LocalSpan, Right.Span);
                        else
                            Span.SetInterval(LocalSpan);
                    }
                }
                // Both children have a span
                else
                {
                    // Right span has higher high than left span
                    if (Right.Span.CompareHigh(Left.Span) > 0)
                        if (LocalSpan.IsEmpty)
                            Span.SetInterval(Left.Span, Right.Span);
                        else
                            Span.SetInterval(Left.Span, Right.Span.HighestHigh(LocalSpan));
                    else
                    {
                        if (LocalSpan.IsEmpty)
                            Span.SetInterval(Left.Span);
                        else
                        {
                            if (Left.Span.CompareHigh(LocalSpan) >= 0)
                                Span.SetInterval(Left.Span);
                            else
                                Span.SetInterval(Left.Span, LocalSpan);
                        }
                    }
                }

                return true;
            }

            public void UpdateMaximum()
            {
                // Set Max to Left's Max
                Max = Left != null ? Left.Max : 0;

                // Start building up the other possible Max sums
                var value = (Left != null ? Left.Sum : 0) + DeltaAt;
                // And check if they are higher the previously found max
                if (value > Max)
                    Max = value;

                // Add DeltaAfter and check for new max
                value += DeltaAfter;
                if (value > Max)
                    Max = value;

                // Add Right's max and check for new max
                value += Right != null ? Right.Max : 0;
                if (value > Max)
                    Max = value;
            }

            public void UpdateMaximumDepth()
            {
                // Set Max to Left's Max
                Max = Left != null ? Left.Max : 0;

                // Start building up the other possible Max sums
                var value = (Left != null ? Left.Sum : 0) + DeltaAt;
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

            public bool AddIntervalToList(I interval)
            {
                Contract.Requires(interval != null);

                Contract.Ensures(LocalSpan != null);

                bool intervalWasAdded;

                // Make copy if no span exists, otherwise join with current span
                if (LocalSpan.IsEmpty)
                    LocalSpan.SetInterval(interval);
                else
                    LocalSpan.ExpandInterval(interval);

                if (interval.LowIncluded)
                {
                    // Create the list if necessary
                    if (IncludedList == null)
                        IncludedList = new IntervalList();

                    intervalWasAdded = IncludedList.Add(interval);

                    // Update delta
                    if (intervalWasAdded)
                        DeltaAt++;
                }
                else
                {
                    // Create the list if necessary
                    if (ExcludedList == null)
                        ExcludedList = new IntervalList();

                    intervalWasAdded = ExcludedList.Add(interval);

                    // Update delta
                    if (intervalWasAdded)
                        DeltaAfter++;
                }

                // Should also return true, as elements added to a bag will always be added
                return intervalWasAdded;
            }

            public bool RemoveIntervalFromList(I interval)
            {
                Contract.Requires(interval != null);

                if (interval.LowIncluded)
                {
                    // Try to remove the interval from the list
                    if (IncludedList == null || !IncludedList.Remove(interval))
                        return false;

                    DeltaAt--;

                    // Remove list if empty
                    if (IncludedList.IsEmpty)
                        IncludedList = null;
                }
                else
                {
                    // Try to remove the interval from the list
                    if (ExcludedList == null || !ExcludedList.Remove(interval))
                        return false;

                    DeltaAfter--;

                    // Remove list if empty
                    if (ExcludedList.IsEmpty)
                        ExcludedList = null;
                }

                // Update span as we removed the interval
                if (IncludedList != null)
                    if (ExcludedList != null)
                    {
                        LocalSpan.SetInterval(IncludedList.Highest);
                        LocalSpan.ExpandInterval(ExcludedList.Highest);
                    }
                    else
                        LocalSpan.SetInterval(IncludedList.Highest);
                else
                {
                    if (ExcludedList != null)
                        LocalSpan.SetInterval(ExcludedList.Highest);
                    else
                        LocalSpan.IsEmpty = true;
                }

                return true;
            }

            public void AddHighToDelta(bool highIncluded)
            {
                if (highIncluded)
                    DeltaAfter--;
                else
                    DeltaAt--;
            }

            public void RemoveHighFromDelta(bool highIncluded)
            {
                if (highIncluded)
                    DeltaAfter++;
                else
                    DeltaAt++;
            }

            public void Swap(Node successor)
            {
                Contract.Requires(successor != null);
                Contract.Requires(LocalSpan.IsEmpty && IncludedList == null && ExcludedList == null);

                Contract.Ensures(Key != null);
                // Ensures the successor is deletable
                Contract.Ensures(successor.IsEmpty);

                // Swap key
                var tmpKey = Key;
                Key = successor.Key;
                successor.Key = tmpKey;

                // Swap local span
                LocalSpan.SetInterval(successor.LocalSpan);
                successor.LocalSpan.IsEmpty = true;

                // Copy successor data to node
                IncludedList = successor.IncludedList;
                successor.IncludedList = null;
                ExcludedList = successor.ExcludedList;
                successor.ExcludedList = null;
                DeltaAfter = successor.DeltaAfter;
                DeltaAt = successor.DeltaAt;

                successor.IsEmpty = true;
            }

            public int CompareTo(Node other)
            {
                return Key.CompareTo(other.Key);
            }

            public override string ToString()
            {
                return Key.ToString();
            }

            public bool ContainsReferenceEqualInterval(I interval)
            {
                Contract.Requires(interval != null);

                return interval.LowIncluded
                           ? IncludedList != null && IncludedList.ContainsReferenceEqualInterval(interval)
                           : ExcludedList != null && ExcludedList.ContainsReferenceEqualInterval(interval);
            }

            #endregion
        }

        #endregion

        #region AVL tree methods

        /// <summary>
        /// Rotates lefts this instance
        /// </summary>
        private static Node rotateLeft(Node root)
        {
            Contract.Requires(root != null);
            Contract.Requires(root.Right != null);

            var node = root.Right;

            root.Right = node.Left;
            root.UpdateSpan();
            root.UpdateMaximumDepth();

            node.Left = root;
            node.UpdateSpan();
            node.UpdateMaximumDepth();

            return node;
        }

        /// <summary>
        /// Rotates right this instance.
        /// </summary>
        private static Node rotateRight(Node root)
        {
            Contract.Requires(root != null);
            Contract.Requires(root.Left != null);

            var node = root.Left;

            root.Left = node.Right;
            root.UpdateSpan();
            root.UpdateMaximumDepth();

            node.Right = root;
            node.UpdateSpan();
            node.UpdateMaximumDepth();

            return node;
        }

        private static Node rotateForAdd(Node root, ref bool updateBalance)
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
                // Node is balanced after the root was added
                case 0:
                    updateBalance = false;
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
                    updateBalance = false;
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

                    updateBalance = false;
                    break;
            }

            return root;
        }

        private static Node rotateForRemove(Node root, ref bool updateBalance)
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
                // HighestHigh will not change for parent, so we can stop here
                case -1:
                case +1:
                    updateBalance = false;
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
                            updateBalance = false;
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
                            updateBalance = false;
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

        #endregion

        #region Constructors

        /// <summary>
        /// Construct an empty Dynamic Interval Tree. Reference duplicates are not allowed.
        /// </summary>
        public DynamicIntervalTree()
        {
            // Set reference duplicate behavior
            AllowsReferenceDuplicates = false;
        }

        /// <summary>
        /// Construct an empty Dynamic Interval Tree.
        /// </summary>
        /// <param name="allowReferenceDuplicates">Set how reference duplicates should be handled.</param>
        public DynamicIntervalTree(bool allowReferenceDuplicates = false)
        {
            // Set reference duplicate behavior
            AllowsReferenceDuplicates = allowReferenceDuplicates;
        }

        /// <summary>
        /// Construct an empty Dynamic Interval Tree that does not allow reference duplicates.
        /// </summary>
        /// <param name="intervals">A collection of intervals.</param>
        public DynamicIntervalTree(IEnumerable<I> intervals, bool allowReferenceDuplicates = false)
        {
            Contract.Requires(intervals != null);

            // Set reference duplicate behavior
            AllowsReferenceDuplicates = allowReferenceDuplicates;

            // TODO: Pre-build the tree structure

            // Insert all intervals
            foreach (var interval in intervals)
                Add(interval);
        }

        #endregion

        #region Collection Value

        /// <inheritdoc/>
        public override bool IsEmpty
        {
            get
            {
                Contract.Ensures(Contract.Result<bool>() == (_root == null));
                return _root == null;
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

            var node = _root;

            while (true)
            {
                if (!node.LocalSpan.IsEmpty)
                    // Choose an interval from one of the lists
                    return node.IncludedList != null ? node.IncludedList.Choose() : node.ExcludedList.Choose();

                // Move left if the node was empty
                node = node.Left;
            }
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
        public bool AllowsOverlaps { get { return true; } }

        /// <inheritdoc/>
        public bool AllowsReferenceDuplicates { get; private set; }

        #endregion

        #region Collection Properties

        /// <inheritdoc/>
        public IInterval<T> Span { get { return new IntervalBase<T>(_root.Span); } }

        /// <inheritdoc/>
        public I LowestInterval { get { return lowestList().Choose(); } }

        /// <inheritdoc/>
        public IEnumerable<I> LowestIntervals { get { return IsEmpty ? Enumerable.Empty<I>() : lowestList(); } }

        private IntervalList lowestList()
        {
            var node = _root;

            // Find the first node
            while (node.Left != null)
                node = node.Left;

            return node.IncludedList ?? node.ExcludedList;
        }

        /// <inheritdoc/>
        public I HighestInterval
        {
            get
            {
                var node = _root;

                while (true)
                {
                    // Check if children contain any intervals
                    var rightDeadEnd = node.Right == null || node.Right.Span.IsEmpty;
                    var leftDeadEnd = node.Left == null || node.Left.Span.IsEmpty;

                    // Check if the node span's high matches the local span's high
                    if (rightDeadEnd && leftDeadEnd || !node.LocalSpan.IsEmpty && node.Span.CompareHigh(node.LocalSpan) == 0)
                        return highestIntervalInNode(node);

                    // Check if right child is a dead end
                    if (rightDeadEnd)
                        node = node.Left;
                    // Check if left child is a dead end
                    else if (leftDeadEnd)
                        node = node.Right;
                    // Both children might contain highest interval
                    else
                        // We prefer right, as this will give us a small interval
                        node = node.Left.Span.CompareHigh(node.Right.Span) <= 0 ? node.Right : node.Left;
                }
            }
        }

        private static I highestIntervalInNode(Node node)
        {
            // If the included list is non-empty, it might hold the highest interval
            if (node.IncludedList != null)
            {
                var highestFromIncludedList = node.IncludedList.Highest;

                // Check if we have the highest interval
                if (highestFromIncludedList.CompareHigh(node.LocalSpan) == 0)
                    return highestFromIncludedList;
            }

            // Otherwise it must be the highest in the excluded list
            return node.ExcludedList.Highest;
        }

        public IEnumerable<I> HighestIntervals
        {
            get
            {
                // TODO: Implement properly!

                if (IsEmpty)
                    return Enumerable.Empty<I>();

                var highestInterval = HighestInterval;

                if (highestInterval.HighIncluded)
                    return FindOverlaps(highestInterval.High);

                return FindOverlaps(highestInterval).Where(x => x.High.CompareTo(highestInterval.High) == 0);
            }
        }

        public int MaximumDepth { get { return IsEmpty ? 0 : _root.Max; } }

        #endregion

        #endregion

        #region Enumerable

        /// <inheritdoc/>
        public override IEnumerator<I> GetEnumerator()
        {
            return Sorted.GetEnumerator();
        }

        public IEnumerable<I> Sorted
        {
            get
            {
                foreach (var node in sortedNodes(_root))
                {
                    if (node.IncludedList != null)
                        foreach (var interval in node.IncludedList)
                            yield return interval;

                    if (node.ExcludedList != null)
                        foreach (var interval in node.ExcludedList)
                            yield return interval;
                }
            }
        }

        [Pure]
        private static IEnumerable<Node> nodes(Node root)
        {
            while (root != null)
            {
                foreach (var node in nodes(root.Left))
                    yield return node;

                yield return root;

                root = root.Right;
            }
        }

        private IEnumerable<Node> sortedNodes(Node root)
        {
            Contract.Ensures(Contract.Result<IEnumerable<Node>>().IsSorted());

            if (IsEmpty)
                yield break;

            var i = 0;
            var stack = new Node[calcHeight(_count)];

            var current = root;
            while (i > 0 || current != null)
            {
                if (current != null)
                {
                    // Push node onto stack and search left
                    stack[i++] = current;
                    current = current.Left;
                }
                else
                {
                    // Pop node and yield, then search right
                    yield return current = stack[--i];
                    current = current.Right;
                }
            }
        }

        [Pure]
        private static int calcHeight(int count)
        {
            // TODO: Should this be one more as the count is intervals, and not endpoints? Can't seem to make it fail.
            return (int) Math.Ceiling(1.44 * Math.Log(count + 2, 2) - 0.328) + 1;
        }

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
            // TODO: Make sorted
            if (IsEmpty)
                yield break;

            var i = 0;
            var stack = new Node[calcHeight(Count)];
            stack[i++] = _root;

            while (i > 0)
            {
                var root = stack[--i];
                // Set to null to avoid memory leak
                stack[i] = null;

                // Query is to the left of root's Low
                var compare = query.High.CompareTo(root.Key);
                if (root.Left != null && (compare < 0 || compare == 0 && (!query.HighIncluded || root.IncludedList == null)))
                {
                    stack[i++] = root.Left;
                }
                else if (!root.Span.IsEmpty && query.CompareLowHigh(root.Span) <= 0)
                {
                    if (root.Left != null)
                        stack[i++] = root.Left;
                    if (root.Right != null)
                        stack[i++] = root.Right;

                    if (!root.LocalSpan.IsEmpty)
                    {
                        // Find overlaps in lists
                        if (root.IncludedList != null)
                            foreach (var interval in root.IncludedList.FindOverlaps(query))
                                yield return interval;

                        if (root.ExcludedList != null)
                            foreach (var interval in root.ExcludedList.FindOverlaps(query))
                                yield return interval;
                    }
                }
            }
        }

        public IEnumerable<I> FindOverlapsSelective(IInterval<T> query)
        {
            if (IsEmpty)
                yield break;

            // TODO: Should this be one more as the count is intervals, and not endpoints? Can't seem to make it fail.
            var height = (int) Math.Ceiling(1.44 * Math.Log(Count + 2, 2) - 0.328);
            var stack = new Node[height];
            var i = 0;

            stack[i++] = _root;

            while (i > 0)
            {
                var root = stack[--i];
                // Set to null to avoid memory leak
                stack[i] = null;

                // Query is to the left of root's Low
                var compare = query.High.CompareTo(root.Key);
                if (root.Left != null && (compare < 0 || compare == 0 && (!query.HighIncluded || root.IncludedList == null)))
                {
                    if (!root.Left.Span.IsEmpty && query.Overlaps(root.Left.Span))
                        stack[i++] = root.Left;
                }
                else if (!root.Span.IsEmpty && query.CompareLowHigh(root.Span) <= 0)
                {
                    if (root.Left != null && !root.Left.Span.IsEmpty && query.CompareLowHigh(root.Left.Span) <= 0)
                        stack[i++] = root.Left;
                    if (root.Right != null)
                        stack[i++] = root.Right;

                    if (!root.LocalSpan.IsEmpty)
                    {
                        // Find overlaps in lists
                        if (root.IncludedList != null)
                            foreach (var interval in root.IncludedList.FindOverlaps(query))
                                yield return interval;

                        if (root.ExcludedList != null)
                            foreach (var interval in root.ExcludedList.FindOverlaps(query))
                                yield return interval;
                    }
                }
            }
        }

        #endregion

        #region Find Overlap

        /// <inheritdoc/>
        public bool FindOverlap(T query, out I overlap)
        {
            bool result;

            using (var enumerator = FindOverlaps(query).GetEnumerator())
                overlap = (result = enumerator.MoveNext()) ? enumerator.Current : null;

            return result;
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
            return FindOverlaps(query).Count();
        }

        /// <inheritdoc/>
        public int CountOverlaps(IInterval<T> query)
        {
            return FindOverlaps(query).Count();
        }

        #endregion

        #region Gaps

        /// <inheritdoc/>}
        public IEnumerable<IInterval<T>> Gaps
        {
            get { return sortedNodes(_root).Select(n => n.LocalSpan).Where(x => !x.IsEmpty).Cast<IInterval<T>>().Gaps(); }
        }

        /// <inheritdoc/>
        public IEnumerable<IInterval<T>> FindGaps(IInterval<T> query)
        {
            return FindOverlaps(query).Gaps(query, false);
        }

        #endregion

        #region Extensible

        /// <inheritdoc/>
        public bool IsReadOnly { get { return false; } }

        #region Add

        /// <inheritdoc/>
        public bool Add(I interval)
        {
            var nodeWasAdded = false;
            var intervalWasAdded = false;

            _root = addLow(interval, _root, ref nodeWasAdded, ref intervalWasAdded);

            if (intervalWasAdded)
            {
                nodeWasAdded = false;
                var updateSpan = false;
                _root = addHigh(interval, _root, ref nodeWasAdded, ref updateSpan);

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

        private Node addLow(I interval, Node root, ref bool nodeWasAdded, ref bool intervalWasAdded)
        {
            Contract.Requires(interval != null);

            if (root == null)
            {
                nodeWasAdded = intervalWasAdded = true;
                return new Node(interval);
            }

            var compare = interval.Low.CompareTo(root.Key);

            if (compare > 0)
            {
                root.Right = addLow(interval, root.Right, ref nodeWasAdded, ref intervalWasAdded);

                if (nodeWasAdded)
                    root.Balance++;
            }
            else if (compare < 0)
            {
                root.Left = addLow(interval, root.Left, ref nodeWasAdded, ref intervalWasAdded);

                if (nodeWasAdded)
                    root.Balance--;
            }
            // Add to node if we allow reference duplicates or if it doesn't already contain it 
            else if (AllowsReferenceDuplicates || !root.ContainsReferenceEqualInterval(interval))
                intervalWasAdded = root.AddIntervalToList(interval);

            if (intervalWasAdded)
            {
                root.UpdateSpan();

                // Increment sum and update max
                root.Sum++;
                root.UpdateMaximum();

                // Tree might be unbalanced after root was added, so we rotate
                if (nodeWasAdded)
                    root = rotateForAdd(root, ref nodeWasAdded);
            }

            return root;
        }

        private static Node addHigh(I interval, Node root, ref bool nodeWasAdded, ref bool updateSpan)
        {
            Contract.Requires(interval != null);

            if (root == null)
            {
                nodeWasAdded = updateSpan = true;
                return new Node(interval.High, interval.HighIncluded);
            }

            var compare = interval.High.CompareTo(root.Key);

            if (compare > 0)
            {
                root.Right = addHigh(interval, root.Right, ref nodeWasAdded, ref updateSpan);

                Contract.Assert(root.Right != null);

                if (nodeWasAdded)
                    root.Balance++;
            }
            else if (compare < 0)
            {
                root.Left = addHigh(interval, root.Left, ref nodeWasAdded, ref updateSpan);

                Contract.Assert(root.Left != null);

                if (nodeWasAdded)
                    root.Balance--;
            }
            else
                root.AddHighToDelta(interval.HighIncluded);

            if (updateSpan)
                updateSpan = root.UpdateSpan();

            // Decrement sum and update max
            root.Sum--;
            root.UpdateMaximum();

            // Tree might be unbalanced after root was added, so we rotate
            if (nodeWasAdded)
                root = rotateForAdd(root, ref nodeWasAdded);

            return root;
        }

        #endregion

        #region Remove

        /// <inheritdoc/>
        public bool Remove(I interval)
        {
            // Nothing to remove is the collection is empty or the interval doesn't overlap the span
            if (IsEmpty || !interval.Overlaps(_root.Span))
                return false;

            // Remove the interval based on low endpoint
            var nodeWasRemoved = false;
            var intervalWasRemoved = false;
            _root = removeLow(interval, _root, ref nodeWasRemoved, ref intervalWasRemoved);

            // If the interval was removed, we need to remove the data associated with the high endpoint as well
            if (intervalWasRemoved)
            {
                nodeWasRemoved = false;
                var updateSpan = false;
                _root = removeHigh(interval, _root, ref nodeWasRemoved, ref updateSpan);

                // Adjust count and throw event
                _count--;
                raiseForRemove(interval);
            }

            return intervalWasRemoved;
        }

        private static Node removeLow(I interval, Node root, ref bool nodeWasDeleted, ref bool intervalWasRemoved)
        {
            Contract.Requires(interval != null);
            Contract.Requires(root != null);

            var compare = interval.Low.CompareTo(root.Key);

            // Search right
            if (compare > 0)
            {
                // If there isn't a right subtree, the collection does not contain the interval
                if (root.Right == null)
                    return root;

                root.Right = removeLow(interval, root.Right, ref nodeWasDeleted, ref intervalWasRemoved);

                // Adjust balance if necessary
                if (nodeWasDeleted)
                    root.Balance--;
            }
            // Search left
            else if (compare < 0)
            {
                // If there isn't a right subtree, the collection does not contain the interval
                if (root.Left == null)
                    return root;

                root.Left = removeLow(interval, root.Left, ref nodeWasDeleted, ref intervalWasRemoved);

                // Adjust balance if necessary
                if (nodeWasDeleted)
                    root.Balance++;
            }
            // Node found
            else
            {
                // Try to remove interval from list
                intervalWasRemoved |= root.RemoveIntervalFromList(interval);

                // If the interval was removed, we might be able to remove the node as well
                if (intervalWasRemoved && root.IsEmpty)
                {
                    // The root has two children
                    if (root.Left != null && root.Right != null)
                    {
                        // Swap the root and successor
                        root.Swap(findSuccessor(root.Right));

                        // Remove the successor node from the right subtree
                        nodeWasDeleted = false;
                        root.Right = removeLow(interval, root.Right, ref nodeWasDeleted, ref intervalWasRemoved);

                        // Adjust balance if necessary
                        if (nodeWasDeleted)
                            root.Balance--;
                    }
                    // At least one child is null
                    else
                    {
                        nodeWasDeleted = true;

                        // We simply remove the node by returning a reference to its child
                        return root.Left ?? root.Right;
                    }
                }
            }

            // If the interval was removed the span might have changed
            if (intervalWasRemoved)
            {
                root.UpdateSpan();
                root.UpdateMaximumDepth();
            }

            // Rotate if necessary
            if (nodeWasDeleted)
                root = rotateForRemove(root, ref nodeWasDeleted);

            return root;
        }

        private static Node removeHigh(I interval, Node root, ref bool nodeWasDeleted, ref bool updateSpan)
        {
            Contract.Requires(interval != null);
            Contract.Requires(root != null);

            var compare = interval.High.CompareTo(root.Key);

            // Search right
            if (compare > 0)
            {
                root.Right = removeHigh(interval, root.Right, ref nodeWasDeleted, ref updateSpan);

                // Adjust balance if necessary
                if (nodeWasDeleted)
                    root.Balance--;
            }
            // Search left
            else if (compare < 0)
            {
                root.Left = removeHigh(interval, root.Left, ref nodeWasDeleted, ref updateSpan);

                // Adjust balance if necessary
                if (nodeWasDeleted)
                    root.Balance++;
            }
            else
            {
                // Update delta for the interval's high
                root.RemoveHighFromDelta(interval.HighIncluded);

                // If the interval was removed, we might be able to remove the node as well
                if (root.IsEmpty)
                {
                    // A node will be removed, which requires updates of spans
                    updateSpan = true;

                    // The root has two children
                    if (root.Left != null && root.Right != null)
                    {
                        // Swap the root and successor
                        root.Swap(findSuccessor(root.Right));

                        // Remove the successor node from the right subtree
                        nodeWasDeleted = false;
                        root.Right = removeHigh(interval, root.Right, ref nodeWasDeleted, ref updateSpan);

                        // Adjust balance if necessary
                        if (nodeWasDeleted)
                            root.Balance--;
                    }
                    // At least one child is null
                    else
                    {
                        nodeWasDeleted = true;

                        // We simply remove the node by returning a reference to its child
                        return root.Left ?? root.Right;
                    }
                }
            }

            if (updateSpan)
                updateSpan = root.UpdateSpan();

            root.UpdateMaximumDepth();

            // Rotate if necessary
            if (nodeWasDeleted)
                root = rotateForRemove(root, ref nodeWasDeleted);

            return root;
        }

        /// <summary>
        /// Find the successor node.
        /// </summary>
        /// <returns>The successor node.</returns>
        private static Node findSuccessor(Node node)
        {
            Contract.Requires(node != null);
            Contract.Ensures(Contract.Result<Node>() != null);
            Contract.Ensures(Contract.Result<Node>() == nodes(Contract.OldValue(node)).First());

            while (node.Left != null)
                node = node.Left;

            return node;
        }

        #endregion

        #region Clear

        /// <inheritdoc/>
        public void Clear()
        {
            Contract.Ensures(_root == null);
            Contract.Ensures(_count == 0);

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
            Contract.Ensures(_root == null);
            Contract.Ensures(_count == 0);

            _root = null;
            _count = 0;
        }

        #endregion

        #endregion

        #endregion
    }
}