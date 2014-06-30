using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace C5.Intervals
{
    /// <summary>
    /// A collection that allows fast overlap queries on collections of intervals.
    /// </summary>
    /// <remarks>The data structures do not support updates on its intervals' values. If you wish to change an interval's endpoints or their inclusion, the interval should be removed from the data structure first, changed and then added again.</remarks>
    /// <typeparam name="I">The interval type in the collection. Especially used for return types for enumeration.</typeparam>
    /// <typeparam name="T">The interval's endpoint values.</typeparam>
    [ContractClass(typeof(IntervalCollectionContract<,>))]
    public interface IIntervalCollection<I, T> : ICollectionValue<I>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Properties

        #region Data Structure Properties

        /// <summary>
        /// Indicates if the collection supports overlapping intervals. If a collection supports
        /// overlaps, it must also support interval duplicate objects - that is other objects
        /// with the same endpoints and inclusions.
        /// </summary>
        /// <value>True if this collection allows overlapping intervals.</value>
        [Pure]
        bool AllowsOverlaps { get; }

        /// <summary>
        /// Indicates if the collection supports object duplicates based on reference equality.
        /// </summary>
        /// <value>True if this collection allows reference duplicates.</value>
        bool AllowsReferenceDuplicates { get; }

        #endregion

        #region Collection Properties

        /// <summary>
        /// The smallest interval that spans all intervals in the collection. The interval's low is
        /// the lowest low endpoint in the collection and the high is the highest high endpoint.
        /// <c>coll.FindOverlaps(coll.Span())</c> will by definition return all intervals in the collection.
        /// </summary>
        /// <remarks>
        /// Not defined for an empty collection.
        /// </remarks>
        /// <returns>The smallest spanning interval.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if called on an empty collection.
        /// </exception>
        [Pure]
        IInterval<T> Span { get; }

        /// <summary>
        /// Returns the interval with the lowest (low) endpoint in the collection. If there are
        /// multiple intervals that share the lowest endpoint, the interval that is fastest to
        /// retrieve will be returned.
        /// </summary>
        /// <remarks>Not defined for an empty collection.</remarks>
        /// <value>An interval with the lowest endpoint in the collection.</value>
        I LowestInterval { get; }

        /// <summary>
        /// Returns all intervals with the highest (high) endpoint in the collection. If the
        /// collection is empty, the enumerable will be empty. If the collection does not allow
        /// overlaps, the result will only contain one interval.
        /// </summary>
        /// <value>All intervals with the highest endpoint in the collection.</value>
        IEnumerable<I> LowestIntervals { get; }

        /// <summary>
        /// Returns the interval with the highest (high) endpoint in the collection. If there are
        /// multiple intervals that share the highest endpoint, the interval that is fastest to
        /// retrieve will be returned.
        /// </summary>
        /// <remarks>Not defined for an empty collection.</remarks>
        /// <value>An interval with the highest endpoint in the collection.</value>
        I HighestInterval { get; }

        /// <summary>
        /// Returns all intervals with the highest (high) endpoint in the collection. If the
        /// collection is empty, the enumerable will be empty. If the collection does not allow
        /// overlaps, the result will only contain one interval.
        /// </summary>
        /// <value>All intervals with the highest endpoint in the collection.</value>
        IEnumerable<I> HighestIntervals { get; }

        /// <summary>
        /// The maximum number of intervals overlapping at a single point in the collection.
        /// </summary>
        /// <remarks>The point of maximum depth may not be representable with an endpoint value, as it could be between two discrete values.</remarks>
        [Pure]
        int MaximumDepth { get; }

        #endregion

        #endregion

        #region Enumerable

        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection in sorted order.
        /// This might be slower than using the normal enumerator, but it ensures that the
        /// intervals are sorted.
        /// </summary>
        [Pure]
        IEnumerable<I> Sorted { get; }

        #endregion

        #region Find Overlaps

        // TODO: Check seealso in documentation. Does it actually reference the other overloaded method? Add it to FindOverlap as well
        // @design: made to spare the user of making a point interval [q:q] and allow for more effective implementations of the interface for some data structures
        /// <summary>
        /// Create an enumerable, enumerating all intervals that overlap the query point.
        /// </summary>
        /// <param name="query">The query point.</param>
        /// <returns>All intervals that overlap the query point.</returns>
        /// <seealso cref="FindOverlaps(IInterval{T})"/>
        IEnumerable<I> FindOverlaps(T query);

        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection that overlap the query interval.
        /// </summary>
        /// <param name="query">The query interval.</param>
        /// <returns>All intervals that overlap the query interval.</returns>
        /// <seealso cref="FindOverlaps(T)"/>
        [Pure]
        IEnumerable<I> FindOverlaps(IInterval<T> query);

        #endregion

        #region Find Overlap

        /// <summary>
        /// Check if there exists an interval that overlaps the query point.
        /// </summary>
        /// <remarks>There is no guaranty of which interval will be returned, but implementations must assure that the interval returned is the fastest to retrieve.</remarks>
        /// <param name="query">The query point.</param>
        /// <param name="overlap">The overlapping interval found, if return is true.</param>
        /// <returns>True if an interval overlapped the query.</returns>
        [Pure]
        bool FindOverlap(T query, out I overlap);

        /// <summary>
        /// Check if there exists an interval that overlaps the query interval.
        /// </summary>
        /// <remarks>There is no guaranty of which interval will be returned, but implementations must assure that the interval returned is the fastest to retrieve.</remarks>
        /// <param name="query">The query interval.</param>
        /// <param name="overlap">The overlapping interval found, if return is true.</param>
        /// <returns>True if an interval overlapped the query.</returns>
        [Pure]
        bool FindOverlap(IInterval<T> query, out I overlap);

        #endregion

        #region Count Overlaps

        /// <summary>
        /// Count the number of intervals overlapping the query point.
        /// </summary>
        /// <param name="query">The query interval</param>
        /// <remarks>Beware that not all data structure support this operation any faster than <c>FindOverlaps.Count()</c>!</remarks>
        /// <returns>The number of intervals that overlap the query point.</returns>
        [Pure]
        int CountOverlaps(T query);

        /// <summary>
        /// Count the number of intervals overlapping the query interval.
        /// Beware that not all data structure support this operation any faster than FindOverlaps.Count().
        /// </summary>
        /// <param name="query">The query interval</param>
        /// <returns>The number of intervals that overlap the query</returns>
        [Pure]
        int CountOverlaps(IInterval<T> query);

        #endregion

        #region Gaps

        // TODO: Improve documentation

        /// <summary>
        /// Find all gaps between the intervals in the collection. The gaps will have no
        /// overlaps with the collection, and all gaps will be contained in the span of the collection.
        /// </summary>
        [Pure]
        IEnumerable<IInterval<T>> Gaps { get; }

        /// <summary>
        /// Find all gaps between the intervals in the collection that overlap the query
        /// interval. The gaps will have no overlaps with the collection, and all gaps will be
        /// contained in the query interval.
        /// </summary>
        /// <param name="query">
        /// Query interval that determines within which intervals the gaps must be.
        /// </param>
        /// <returns>
        /// Gaps contained in the query interval, not overlapping any of the intervals in the collection.
        /// </returns>
        [Pure]
        IEnumerable<IInterval<T>> FindGaps(IInterval<T> query);

        #endregion

        #region Extensible

        /// <summary>
        /// If true any call of an updating operation will throw a
        /// <code>ReadOnlyCollectionException</code>.
        /// </summary>
        /// <value>True if this collection is read-only.</value>
        [Pure]
        bool IsReadOnly { get; }

        /// <summary>
        /// Add an interval to the collection.
        /// </summary>
        /// <remarks>Different implementations may handle duplicates differently.</remarks>
        /// <param name="interval">The interval to add.</param>
        /// <returns>True if the interval was added.</returns>
        bool Add(I interval);

        /// <summary>
        /// Add a collection of intervals to the collection.
        /// </summary>
        /// <remarks>Different implementations may handle duplicates differently.</remarks>
        /// <param name="intervals">The intervals to add.</param>
        void AddAll(IEnumerable<I> intervals);

        /// <summary>
        /// Remove an interval from the collection.
        /// </summary>
        /// <remarks>Different implementations may remove duplicates differently.</remarks>
        /// <param name="interval">The interval to remove.</param>
        /// <returns>True if the interval was removed.</returns>
        bool Remove(I interval);

        /// <summary>
        /// Remove all intervals from the collection.
        /// </summary>
        void Clear();

        #endregion
    }

    // TODO: Add helpful strings to code contracts instead of displaying the actual contract
    [ContractClassFor(typeof(IIntervalCollection<,>))]
    internal abstract class IntervalCollectionContract<I, T> : IIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Properties

        #region Data Structure Properties

        public abstract bool AllowsOverlaps { get; }

        public bool AllowsReferenceDuplicates
        {
            get
            {
                // If the collection doesn't support overlaps it can't support reference duplicates either
                Contract.Ensures(!Contract.Result<bool>() || AllowsOverlaps);

                throw new NotImplementedException();
            }
        }

        #endregion

        #region Collection Properties

        public IInterval<T> Span
        {
            get
            {
                Contract.Requires(!IsEmpty, "An empty collection has no span.");

                // Span is not null
                Contract.Ensures(Contract.Result<IInterval<T>>() != null);
                // Span contains all intervals
                Contract.Ensures(Contract.ForAll(this, x => Contract.Result<IInterval<T>>().Contains(x)));
                // There is an interval that has the same low as span
                Contract.Ensures(Contract.Exists(this, x => Contract.Result<IInterval<T>>().CompareLow(x) == 0));
                // There is an interval that has the same high as span
                Contract.Ensures(Contract.Exists(this, x => Contract.Result<IInterval<T>>().CompareHigh(x) == 0));

                throw new NotImplementedException();
            }
        }

        public I LowestInterval
        {
            get
            {
                Contract.Requires(!IsEmpty);

                Contract.Ensures(Contract.Result<I>() != null);
                Contract.Ensures(Contract.ForAll(this, x => Contract.Result<I>().CompareLow(x) <= 0));
                Contract.Ensures(Contract.Exists(this, x => ReferenceEquals(x, Contract.Result<I>())));

                throw new NotImplementedException();
            }
        }

        public IEnumerable<I> LowestIntervals
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
                Contract.Ensures(IsEmpty != Contract.Result<IEnumerable<I>>().Any());
                Contract.Ensures(AllowsOverlaps || Contract.Result<IEnumerable<I>>().Count() == (IsEmpty ? 0 : 1));
                Contract.Ensures(Contract.ForAll(this, x => Contract.ForAll(Contract.Result<IEnumerable<I>>(), y => y.CompareLow(x) <= 0)));
                Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<I>>(), x => Contract.Exists(this, y => ReferenceEquals(x, y))));

                throw new NotImplementedException();
            }
        }

        public I HighestInterval
        {
            get
            {
                Contract.Requires(!IsEmpty);

                Contract.Ensures(Contract.Result<I>() != null);
                Contract.Ensures(Contract.ForAll(this, x => x.CompareHigh(Contract.Result<I>()) <= 0));
                Contract.Ensures(Contract.Exists(this, x => ReferenceEquals(x, Contract.Result<I>())));

                throw new NotImplementedException();
            }
        }

        public IEnumerable<I> HighestIntervals
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
                Contract.Ensures(IsEmpty == !Contract.Result<IEnumerable<I>>().Any());
                Contract.Ensures(AllowsOverlaps || Contract.Result<IEnumerable<I>>().Count() == (IsEmpty ? 0 : 1));
                Contract.Ensures(Contract.ForAll(this, x => Contract.ForAll(Contract.Result<IEnumerable<I>>(), y => x.CompareHigh(y) <= 0)));
                Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<I>>(), x => Contract.Exists(this, y => ReferenceEquals(x, y))));

                throw new NotImplementedException();
            }
        }

        public int MaximumDepth
        {
            get
            {
                // Result must be non-negative
                Contract.Ensures(Contract.Result<int>() >= 0);
                // Must be at least one if collection is not empty
                Contract.Ensures(IsEmpty ^ Contract.Result<int>() > 0);
                // If no overlaps are allowed the maximum depth will always be 1, if not empty
                Contract.Ensures(AllowsOverlaps || Contract.Result<int>() == (IsEmpty ? 0 : 1));
                // Result cannot be larger than collection size
                Contract.Ensures(Contract.Result<int>() <= Count);
                // Check result
                Contract.Ensures(Contract.Result<int>() == IntervalCollectionContractHelper.MNO<I, T>(this));

                throw new NotImplementedException();
            }
        }

        #endregion

        #endregion

        #region Enumerable

        public IEnumerable<I> Sorted
        {
            get
            {
                Contract.Ensures(IsEmpty != Contract.Result<IEnumerable<I>>().Any());

                // The intervals are sorted
                Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I, T>());
                // The enumerator is equal to the normal enumerator
                Contract.Ensures(IntervalCollectionContractHelper.CollectionEquals(this, Contract.Result<IEnumerable<I>>()));

                throw new NotImplementedException();
            }
        }

        #endregion

        #region Find Overlaps

        public IEnumerable<I> FindOverlaps(T query)
        {
            // Query point cannot be null
            Contract.Requires(query != null);

            // The collection of intervals that overlap the query must be equal to the result
            Contract.Ensures(IntervalCollectionContractHelper.CollectionEquals(this.Where(x => x.Overlaps(query)), Contract.Result<IEnumerable<I>>()));
            // All intervals in the collection that do not overlap cannot by in the result
            Contract.Ensures(Contract.ForAll(this.Where(x => !x.Overlaps(query)), x => Contract.ForAll(Contract.Result<IEnumerable<I>>(), y => !ReferenceEquals(x, y))));
            // If the collection doesn't allow overlaps, there can at most be one overlap with the query
            Contract.Ensures(AllowsOverlaps || Contract.Result<IEnumerable<I>>().Count() <= 1);

            throw new NotImplementedException();
        }

        public IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            // Query interval cannot be null
            Contract.Requires(query != null);

            // The collection of intervals that overlap the query must be equal to the result
            Contract.Ensures(IntervalCollectionContractHelper.CollectionEquals(this.Where(x => x.Overlaps(query)), Contract.Result<IEnumerable<I>>()));
            // All intervals in the collection that do not overlap cannot by in the result
            Contract.Ensures(Contract.ForAll(this.Where(x => !x.Overlaps(query)), x => Contract.ForAll(Contract.Result<IEnumerable<I>>(), y => !ReferenceEquals(x, y))));

            throw new NotImplementedException();
        }

        #endregion

        #region Find Overlap

        public bool FindOverlap(T query, out I overlap)
        {
            Contract.Requires(query != null);

            // Result is true if the collection contains an overlap
            Contract.Ensures(Contract.Result<bool>() == Contract.Exists(this, x => x.Overlaps(query)));

            // A found overlap is not null and overlaps query
            Contract.Ensures(!Contract.Result<bool>() || Contract.ValueAtReturn(out overlap) != null && Contract.ValueAtReturn(out overlap).Overlaps(query));

            throw new NotImplementedException();
        }

        public bool FindOverlap(IInterval<T> query, out I overlap)
        {
            Contract.Requires(query != null);

            // Result is true if the collection contains an overlap
            Contract.Ensures(Contract.Result<bool>() == Contract.Exists(this, x => x.Overlaps(query)));

            // A found overlap is not null and overlaps query
            Contract.Ensures(!Contract.Result<bool>() || Contract.ValueAtReturn(out overlap) != null && Contract.ValueAtReturn(out overlap).Overlaps(query));

            throw new NotImplementedException();
        }

        #endregion

        #region Count Overlaps

        public int CountOverlaps(T query)
        {
            Contract.Requires(query != null);

            // Result must be non-negative
            Contract.Ensures(Contract.Result<int>() >= 0);
            // Result cannot be larger than collection size
            Contract.Ensures(Contract.Result<int>() <= Count);
            // Result is equal to the number of intervals in the collection that overlap the query
            Contract.Ensures(Contract.Result<int>() == this.Count(x => IntervalExtensions.Overlaps(x, query)));

            throw new NotImplementedException();
        }

        public int CountOverlaps(IInterval<T> query)
        {
            Contract.Requires(query != null);

            // Result must be non-negative
            Contract.Ensures(Contract.Result<int>() >= 0);
            // Result cannot be larger than collection size
            Contract.Ensures(Contract.Result<int>() <= Count);
            // Result is equal to the number of intervals in the collection that overlap the query
            Contract.Ensures(Contract.Result<int>() == this.Count(x => IntervalExtensions.Overlaps(x, query)));

            throw new NotImplementedException();
        }

        #endregion

        #region Gaps

        // TODO: Make a contract that ensures all gaps have been found. Maybe something with collection + gaps = no gaps

        /// <inheritdoc/>
        public IEnumerable<IInterval<T>> Gaps
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<IInterval<T>>>() != null);
                // If the collection has one or fewer intervals, the result is empty
                Contract.Ensures(Count > 1 || !Contract.Result<IEnumerable<IInterval<T>>>().Any());
                // The gaps don't overlap any interval in the collection
                Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IInterval<T>>>(), x => !x.OverlapsAny(this)));
                // The gaps are contained in the span
                Contract.Ensures(IsEmpty || Contract.ForAll(Contract.Result<IEnumerable<IInterval<T>>>(), Span.Contains));
                // Each gap is met by an interval in the collection
                Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IInterval<T>>>(), x => IntervalCollectionContractHelper.MetByAny(x, this.Cast<IInterval<T>>())));
                // Each gap meets an interval in the collection
                Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IInterval<T>>>(), x => IntervalCollectionContractHelper.MeetsAny(x, this.Cast<IInterval<T>>())));
                // Gaps are sorted
                Contract.Ensures(Contract.Result<IEnumerable<IInterval<T>>>().IsSorted<IInterval<T>, T>());
                // Gaps do not overlap
                Contract.Ensures(Contract.Result<IEnumerable<IInterval<T>>>().ForAllConsecutiveElements((x, y) => !x.Overlaps(y)));

                throw new NotImplementedException();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<IInterval<T>> FindGaps(IInterval<T> query)
        {
            Contract.Requires(query != null);

            Contract.Ensures(Contract.Result<IEnumerable<IInterval<T>>>() != null);
            // The gaps don't overlap any interval in the collection
            Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IInterval<T>>>(), x => !x.OverlapsAny(this)));
            // The gaps are contained in the query
            Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IInterval<T>>>(), query.Contains));
            // Each gap is met by an interval in the collection
            Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IInterval<T>>>(), x => x.CompareLow(query) == 0 || IntervalCollectionContractHelper.MetByAny(x, this.Cast<IInterval<T>>())));
            // Each gap meets an interval in the collection
            Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IInterval<T>>>(), x => x.CompareHigh(query) == 0 || IntervalCollectionContractHelper.MeetsAny(x, this.Cast<IInterval<T>>())));
            // Gaps are sorted
            Contract.Ensures(Contract.Result<IEnumerable<IInterval<T>>>().IsSorted<IInterval<T>, T>());
            // Gaps do not overlap
            Contract.Ensures(Contract.Result<IEnumerable<IInterval<T>>>().ForAllConsecutiveElements((x, y) => !x.Overlaps(y)));

            throw new NotImplementedException();
        }

        #endregion

        #region Extensible

        public abstract bool IsReadOnly { get; }

        public bool Add(I interval)
        {
            Contract.Requires(interval != null);

            // Throws exception if it is read only
            Contract.EnsuresOnThrow<ReadOnlyCollectionException>(IsReadOnly);

            // The collection cannot be empty afterwards
            Contract.Ensures(!IsEmpty);

            // The collection contains the interval
            Contract.Ensures(!AllowsOverlaps || Contract.Exists(this, x => ReferenceEquals(x, interval)));
            // If the interval was added, the number of object with the same reference goes up by one
            Contract.Ensures(Contract.Result<bool>() == (this.Count(x => ReferenceEquals(x, interval)) == Contract.OldValue(this.Count(x => ReferenceEquals(x, interval))) + 1));
            // If the interval wasn't added, the number of object with the same reference stays the same
            Contract.Ensures(Contract.Result<bool>() != (this.Count(x => ReferenceEquals(x, interval)) == Contract.OldValue(this.Count(x => ReferenceEquals(x, interval)))));
            // If the interval is added the count goes up by one
            Contract.Ensures(Contract.Result<bool>() == (Count == Contract.OldValue(Count) + 1));
            // If the interval is not added the count stays the same
            Contract.Ensures(Contract.Result<bool>() != (Count == Contract.OldValue(Count)));

            // If the collection allows reference duplicates, the object will always be added
            Contract.Ensures(!AllowsReferenceDuplicates || Contract.Result<bool>());
            // If the collection doesn't allow reference duplicates, the object should only be added if it didn't contain the object
            Contract.Ensures(AllowsReferenceDuplicates || !AllowsOverlaps || Contract.Result<bool>() != Contract.OldValue(Contract.Exists(this, x => ReferenceEquals(x, interval))));

            throw new NotImplementedException();
        }

        public void AddAll(IEnumerable<I> intervals)
        {
            Contract.Requires(intervals != null);

            // Throws exception if it is read only
            Contract.EnsuresOnThrow<ReadOnlyCollectionException>(IsReadOnly);

            Contract.Ensures(IntervalCollectionContractHelper.ConfirmAddAll<I, T>(Contract.OldValue(Enumerable.ToArray(this)), Enumerable.ToArray(this), intervals, AllowsReferenceDuplicates));

            // The collection contains all intervals
            Contract.Ensures(Contract.ForAll(intervals, x => Contract.Exists(this, y => ReferenceEquals(x, y))));
            // If it allows reference duplicates the count increases with the number of intervals added
            Contract.Ensures(!AllowsReferenceDuplicates || Count == Contract.OldValue(Count) + intervals.Count());
            // If it doesn't allow reference duplicates the count increases with the number of distinct intervals added
            Contract.Ensures(AllowsReferenceDuplicates || Count == Contract.OldValue(Count) + intervals.Distinct(ComparerFactory<I>.CreateEqualityComparer((x, y) => ReferenceEquals(x, y), x => x.GetHashCode())).Count(x => !Contract.OldValue(this.Contains(x))));
            // If intervals is empty, the count is unchanged
            Contract.Ensures(intervals.Any() || Count == Contract.OldValue(Count));


            throw new NotImplementedException();
        }

        public bool Remove(I interval)
        {
            Contract.Requires(interval != null);

            // Throws exception if it is read only
            Contract.EnsuresOnThrow<ReadOnlyCollectionException>(IsReadOnly);

            // Nothing to remove if the collection is empty
            Contract.Ensures(!Contract.OldValue(IsEmpty) || !Contract.Result<bool>());

            // If the interval is removed the count goes down by one
            Contract.Ensures(Contract.Result<bool>() == (Count == Contract.OldValue(Count) - 1));
            // If the interval isn't removed the count stays the same
            Contract.Ensures(Contract.Result<bool>() != (Count == Contract.OldValue(Count)));
            // If the interval was removed, the number of object with the same reference goes down by one
            Contract.Ensures(Contract.Result<bool>() == (this.Count(x => ReferenceEquals(x, interval)) == Contract.OldValue(this.Count(x => ReferenceEquals(x, interval))) - 1));
            // If the interval wasn't removed, the number of object with the same reference stays the same
            Contract.Ensures(Contract.Result<bool>() != (this.Count(x => ReferenceEquals(x, interval)) == Contract.OldValue(this.Count(x => ReferenceEquals(x, interval)))));

            // The result is true if the collection contained the interval
            Contract.Ensures(Contract.Result<bool>() == Contract.OldValue(Contract.Exists(this, x => ReferenceEquals(x, interval))));
            // If the collection doesn't allow reference duplicates, the interval cannot be in the collection afterwards
            Contract.Ensures(AllowsReferenceDuplicates || !Contract.Exists(this, x => ReferenceEquals(x, interval)));

            // If we don't allow reference duplicates, then the interval cannot be in the collection afterwards
            Contract.Ensures(AllowsReferenceDuplicates || !Contract.Exists(this, x => ReferenceEquals(x, interval)));

            throw new NotImplementedException();
        }

        public void Clear()
        {
            // Throws exception if it is read only
            Contract.EnsuresOnThrow<ReadOnlyCollectionException>(IsReadOnly);

            // The collection must be empty afterwards
            Contract.Ensures(IsEmpty);

            throw new NotImplementedException();
        }

        #endregion

        #region Non-interval methods

        public abstract IEnumerator<I> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() { throw new NotImplementedException(); }
        public abstract bool IsEmpty { get; }
        public abstract int Count { get; }
        public abstract I Choose();
        public abstract string ToString(string format, IFormatProvider formatProvider);
        public abstract bool Show(StringBuilder stringbuilder, ref int rest, IFormatProvider formatProvider);
        public abstract EventTypeEnum ListenableEvents { get; }
        public abstract EventTypeEnum ActiveEvents { get; }
        public abstract event CollectionChangedHandler<I> CollectionChanged;
        public abstract event CollectionClearedHandler<I> CollectionCleared;
        public abstract event ItemsAddedHandler<I> ItemsAdded;
        public abstract event ItemInsertedHandler<I> ItemInserted;
        public abstract event ItemsRemovedHandler<I> ItemsRemoved;
        public abstract event ItemRemovedAtHandler<I> ItemRemovedAt;
        public abstract Speed CountSpeed { get; }
        public abstract void CopyTo(I[] array, int index);
        public abstract I[] ToArray();
        public abstract void Apply(Action<I> action);
        public abstract bool Exists(Func<I, bool> predicate);
        public abstract bool Find(Func<I, bool> predicate, out I item);
        public abstract bool All(Func<I, bool> predicate);
        public abstract IEnumerable<I> Filter(Func<I, bool> filter);

        #endregion
    }

    internal static class IntervalCollectionContractHelper
    {
        [Pure]
        public static bool CollectionEquals<I>(IEnumerable<I> expected, IEnumerable<I> actual)
        {
            // Copy to list
            var expectedList = new ArrayList<I>();
            expectedList.AddAll(expected);

            // Copy to list
            var comparer = ComparerFactory<I>.CreateEqualityComparer((x, y) => ReferenceEquals(x, y), x => x.GetHashCode());
            var actualList = new ArrayList<I>(comparer);
            actualList.AddAll(actual);

            // They must be of equal length
            if (expectedList.Count != actualList.Count)
                return false;

            foreach (var interval in expectedList)
                if (!actualList.Remove(interval))
                    return false;

            return actualList.IsEmpty;
        }

        [Pure]
        public static bool CollectionIntervalEquals<I, T>(IEnumerable<I> expected, IEnumerable<I> actual)
            where I : IInterval<T>
            where T : IComparable<T>
        {
            // Copy to list
            var actualList = new ArrayList<I>();
            actualList.AddAll(actual);

            var expectedList = new ArrayList<I>();
            expectedList.AddAll(expected);

            if (actualList.Count != expectedList.Count)
                return false;

            return !actualList.Where((t, i) => !t.IntervalEquals(expectedList[i])).Any();
        }

        [Pure]
        public static int CountOverlaps<T>(IEnumerable<IInterval<T>> intervals, IInterval<T> query)
            where T : IComparable<T>
        {
            Contract.Requires(query != null);
            Contract.Requires(intervals != null);

            return intervals.Count(x => x.Overlaps(query));
        }

        [Pure]
        public static bool MetByAny<T>(IInterval<T> interval, IEnumerable<IInterval<T>> intervals) where T : IComparable<T>
        {
            Contract.Requires(interval != null);
            Contract.Requires(intervals != null);
            Contract.Requires(Contract.ForAll(intervals, x => x != null));

            return intervals.Any(y => y.High.CompareTo(interval.Low) == 0 && y.HighIncluded != interval.LowIncluded);
        }

        [Pure]
        public static bool MeetsAny<T>(IInterval<T> interval, IEnumerable<IInterval<T>> intervals) where T : IComparable<T>
        {
            Contract.Requires(interval != null);
            Contract.Requires(intervals != null);
            Contract.Requires(Contract.ForAll(intervals, x => x != null));

            return intervals.Any(y => interval.High.CompareTo(y.Low) == 0 && interval.HighIncluded != y.LowIncluded);
        }

        [Pure]
        public static bool ConfirmAddAll<I, T>(I[] oldCollection, I[] newCollection, IEnumerable<I> intervalsAdded, bool allowsReferenceDuplicates)
            where I : IInterval<T>
            where T : IComparable<T>
        {
            var comparer = ComparerFactory<I>.CreateEqualityComparer((x, y) => ReferenceEquals(x, y), x => x.GetHashCode());
            var counter = new HashDictionary<I, int>(comparer);

            if (allowsReferenceDuplicates)
            {

                foreach (var interval in intervalsAdded)
                {
                    if (!counter.Contains(interval))
                        counter[interval] = 0;

                    counter[interval]++;

                }
                foreach (var interval in oldCollection)
                {
                    if (!counter.Contains(interval))
                        counter[interval] = 0;

                    counter[interval]++;
                }

                foreach (var interval in newCollection)
                    counter[interval]--;

                if (counter.Any(keyValuePair => keyValuePair.Value != 0))
                    return false;
            }
            else
            {
                foreach (var interval in newCollection)
                {
                    if (!counter.Contains(interval))
                        counter[interval] = 0;

                    counter[interval]++;
                }

                if (intervalsAdded.Any(x => counter[x] != 1))
                    return false;

                if (counter.Any(keyValuePair => keyValuePair.Value != 1))
                    return false;
            }

            return true;
        }

        [Pure]
        public static int MNO<I, T>(IEnumerable<I> intervals)
            where I : IInterval<T>
            where T : IComparable<T>
        {
            Contract.Requires(intervals != null);

            // Check Maximum Depth is correct
            var max = 0;

            // Get intervals and sort them by low, then high, endpoint
            var sortedIntervals = intervals.ToArray();
            var intervalComparer = ComparerFactory<I>.CreateComparer((x, y) => x.CompareTo(y));
            Sorting.IntroSort(sortedIntervals, 0, sortedIntervals.Length, intervalComparer);

            // Create queue sorted on high intervals
            var highComparer = ComparerFactory<IInterval<T>>.CreateComparer(IntervalExtensions.CompareHigh);
            var queue = new IntervalHeap<IInterval<T>>(highComparer);

            // Loop through intervals in sorted order
            foreach (var interval in sortedIntervals)
            {
                // Remove all intervals from the queue not overlapping the current interval
                while (!queue.IsEmpty && interval.CompareLowHigh(queue.FindMin()) > 0)
                    queue.DeleteMin();

                queue.Add(interval);

                if (queue.Count > max)
                {
                    max = queue.Count;
                }
            }

            return max;
        }
    }
}