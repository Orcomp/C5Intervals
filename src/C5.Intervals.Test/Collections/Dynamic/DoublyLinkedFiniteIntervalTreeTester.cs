using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace C5.Intervals.Tests
{
    namespace DoublyLinkedFiniteIntervalTree
    {
        using Interval = IntervalBase<int>;

        // TODO: Re-enable all black-box tests

        #region Black-box

        class DoublyLinkedFiniteIntervalTreeTester_BlackBox : IntervalCollectionTester
        {
            protected override Type GetCollectionType()
            {
                return typeof(DoublyLinkedFiniteIntervalTree<,>);
            }

            protected override Speed CountSpeed()
            {
                return Speed.Constant;
            }

            protected override bool AllowsReferenceDuplicates()
            {
                return false;
            }
        }

        #endregion


        #region White-box

        [TestFixture]
        class DoublyLinkedFiniteIntervalTreeTester_WhiteBox
        {
            #region Meta

            EqualConstraint IsZero
            {
                get
                {
                    return Is.EqualTo(0);
                }
            }

            EqualConstraint IsOne
            {
                get
                {
                    return Is.EqualTo(1);
                }
            }

            class Collection
            {
                private IEnumerable<Interval> _intervals;
                private int? _count;
                private Random _random;

                public Collection()
                {
                    // TODO: Should this be a fix seed to ensure repeatability?
                    _random = new Random(0);
                }

                public Collection(Random random)
                {
                    _random = random;
                }

                public Collection ThatIsEmpty()
                {
                    _count = 0;

                    return this;
                }

                public Collection WithIntervals(IEnumerable<Interval> intervals)
                {
                    _intervals = intervals;

                    return this;
                }

                public Collection WithIntervals(params Interval[] intervals)
                {
                    _intervals = intervals;

                    return this;
                }

                public Collection WithCount(int count)
                {
                    _count = count;

                    return this;
                }

                public DoublyLinkedFiniteIntervalTree<Interval, int> Build()
                {
                    if (_count != null && _count == 0)
                        return new DoublyLinkedFiniteIntervalTree<Interval, int>();

                    // No specific intervals selected
                    else
                    {
                        if (_count == null)
                            _count = _count ?? _random.Next(10, 20);

                        _intervals = Enumerable.Range(0, (int) _count).Select(x => new IntervalBuilder().Build());

                        return new DoublyLinkedFiniteIntervalTree<Interval, int>(_intervals);
                    }
                }

                public Collection WithOneInterval()
                {
                    _count = 1;

                    return this;
                }

                public Collection WithManyIntervals(int? count = null)
                {
                    _count = count;

                    // TODO: Generate random intervals

                    return this;
                }
            }

            class IntervalBuilder
            {
                private int? _low;
                private int? _high;
                private bool? _lowIncluded;
                private bool? _highIncluded;

                public IntervalBuilder() { }

                public IntervalBuilder WithLow(int low)
                {
                    _low = low;

                    return this;
                }

                public IntervalBuilder WithHigh(int high)
                {
                    _high = high;

                    return this;
                }

                public IntervalBuilder Closed()
                {
                    _lowIncluded = _highIncluded = true;

                    return this;
                }

                public IntervalBuilder Open()
                {
                    _lowIncluded = _highIncluded = false;

                    return this;
                }

                public IntervalBuilder LeftClosed()
                {
                    _lowIncluded = true;
                    _highIncluded = false;

                    return this;
                }

                public Interval Build()
                {
                    Contract.Ensures(Contract.Result<Interval>().IsValidInterval());

                    // TODO: Generate interval from endpoint values
                    var low = _low ?? 0;
                    var high = _high ?? 1;
                    var lowIncluded = _lowIncluded ?? true;
                    var highIncluded = _highIncluded ?? false;

                    return new Interval(low, high, lowIncluded, highIncluded);
                }

                public static implicit operator Interval(IntervalBuilder intervalBuilder)
                {
                    return intervalBuilder.Build();
                }
            }

            #endregion

            #region Code Contracts
            // TODO
            #endregion

            #region Inner Classes - Node

            #region Constructors
            // TODO
            #endregion

            #region Public Methods
            // TODO
            #endregion

            #endregion

            #region Constructors
            // TODO
            #endregion

            #region Collection Value
            // TODO
            #endregion

            #region Enumerable
            // TODO
            #endregion

            #region Interval Collection

            #region Properties

            #region Span
            // TODO
            #endregion

            #region Maximum Depth

            [Test]
            [Category("Maximum Depth")]
            public void MaximumDepth_EmptyCollection_Zero()
            {
                var collection = new Collection().ThatIsEmpty().Build();

                Assert.That(collection.MaximumDepth, IsZero);
            }

            [Test]
            [Category("Maximum Depth")]
            public void MaximumDepth_SingleIntervalCollection_One()
            {
                var collection = new Collection().WithOneInterval().Build();

                Assert.That(collection.MaximumDepth, IsOne);
            }

            [Test]
            [Category("Maximum Depth")]
            public void MaximumDepth_NonEmptyCollection_One()
            {
                var collection = new Collection().WithManyIntervals().Build();

                Assert.That(collection.MaximumDepth, IsOne);
            }

            #endregion

            #region Allows Overlaps

            [Test]
            [Category("Allows Overlaps")]
            public void AllowsOverlaps()
            {
                var collection = new Collection().ThatIsEmpty().Build();

                Assert.That(collection.AllowsOverlaps, Is.False);
            }

            #endregion

            #region Allows Reference Duplicates

            [Test]
            [Category("Allows Reference Duplicates")]
            public void AllowsReferenceDuplicates()
            {
                var collection = new Collection().ThatIsEmpty().Build();

                Assert.That(collection.AllowsReferenceDuplicates, Is.False);
            }
            #endregion

            #region Sorted
            // TODO
            #endregion

            #endregion

            #region Find Overlaps
            // TODO
            #endregion

            #region Find Overlap
            // TODO
            #endregion

            #region Count Overlaps
            // TODO
            #endregion

            #region Gaps
            // TODO
            #endregion

            #region Extensible

            #region Is Read Only

            [Test]
            [Category("Is Read Only")]
            public void IsReadOnly()
            {
                var collection = new Collection().ThatIsEmpty().Build();

                Assert.That(collection.IsReadOnly, Is.Not.True);
            }

            #endregion

            #region Add
            // TODO
            #endregion

            #region Remove
            // TODO
            #endregion

            #region Clear

            [Test]
            [Category("Clear")]
            public void Clear_IfIsEmpty_True()
            {
                var collection = new Collection().ThatIsEmpty().Build();

                collection.Clear();

                Assert.That(collection.IsEmpty, Is.True);
            }

            [Test]
            [Category("Clear")]
            public void Clear_IfActiveEventsAndEventTypeEnumClearedAndChangedIsNotZero_False()
            {
                var eventWasRaised = false;
                var collection = new Collection().WithManyIntervals().Build();

                collection.ItemsAdded += (c, args) => eventWasRaised = true;
                collection.ItemsRemoved += (c, args) => eventWasRaised = true;

                collection.Clear();

                Assert.That(collection.IsEmpty, Is.True);
                Assert.That(eventWasRaised, Is.False);
            }

            [Test]
            [Category("Clear")]
            public void Clear_IfActiveEventsAndEventTypeEnumClearedIsNotZero_True()
            {
                var eventWasRaised = false;
                var collection = new Collection().WithManyIntervals().Build();

                collection.CollectionCleared += (sender, args) => eventWasRaised = true;

                collection.Clear();

                Assert.That(collection.IsEmpty, Is.True);
                Assert.That(eventWasRaised, Is.True);
            }

            [Test]
            [Category("Clear")]
            public void Clear_IfActiveEventsAndEventTypeEnumChangedIsNotZero_True()
            {
                var eventWasRaised = false;
                var collection = new Collection().WithManyIntervals().Build();

                collection.CollectionChanged += sender => eventWasRaised = true;

                collection.Clear();

                Assert.That(collection.IsEmpty, Is.True);
                Assert.That(eventWasRaised, Is.True);
            }

            #endregion

            #endregion

            #endregion

            #region QuickGraph
            // TODO
            #endregion
        }

        /*
        [TestFixture]
        class DoublyLinkedFiniteIntervalTreeTester
        {
            [Test]
            public void Add()
            {
                var tree = new DoublyLinkedFiniteIntervalTree<IInterval<int>, int>();

                const int count = 10;
                var numbers = new int[count];
                for (var i = 0; i < count; i++)
                    numbers[i] = i;
                numbers.Shuffle();

                foreach (var number in numbers)
                {
                    tree.Add(new IntervalBase<int>(number));
                }

#if DEBUG
                Console.Out.WriteLine(tree.QuickGraph);
#endif
            }

            [Test]
            public void AddSorted()
            {
                var tree = new DoublyLinkedFiniteIntervalTree<IInterval<int>, int>();

                const int count = 10;

                for (var i = 0; i < count; i++)
                    tree.Add(new IntervalBase<int>(i));

#if DEBUG
                Console.Out.WriteLine(tree.QuickGraph);
#endif
            }

            [Test]
            public void AddBalanced()
            {
                var tree = new DoublyLinkedFiniteIntervalTree<IInterval<int>, int>();

                foreach (var number in balancedNumbers(0, (int) Math.Pow(2, 4) - 2))
                    tree.Add(new IntervalBase<int>(number));

#if DEBUG
                Console.Out.WriteLine(tree.QuickGraph);
#endif
            }

            private IEnumerable<IntervalBase<int>> balancedNumbers(int lower, int upper)
            {
                if (lower > upper)
                    yield break;

                var mid = lower + (upper - lower >> 1);

                yield return new IntervalBase<int>(mid);

                foreach (var interval in balancedNumbers(lower, mid - 1))
                    yield return interval;
                foreach (var interval in balancedNumbers(mid + 1, upper))
                    yield return interval;
            }

            [Test]
            public void RemoveBalanced()
            {
                var tree = new DoublyLinkedFiniteIntervalTree<IInterval<int>, int>();
                int count = (int) Math.Pow(2, 4) - 2;
                var intervals = balancedNumbers(0, count).ToArray();

                foreach (var interval in intervals)
                    tree.Add(interval);

                tree.Remove(intervals[3]);
                tree.Remove(intervals[7]);

#if DEBUG
                Console.Out.WriteLine(tree.QuickGraph);
#endif
            }

            [Test]
            public void AddAndRemove()
            {
                var tree = new DoublyLinkedFiniteIntervalTree<IInterval<int>, int>();

                const int count = 10;

                var intervals = Enumerable.Range(0, count).Select(x => new IntervalBase<int>(x)).ToArray();

                foreach (var interval in intervals)
                    tree.Add(interval);

                Assert.AreEqual(count, tree.Count);

                foreach (var interval in intervals)
                    tree.Remove(interval);

                CollectionAssert.IsEmpty(tree);
                Assert.AreEqual(0, tree.Count);
            }

            [Test]
            public void DoubleAdd()
            {
                var tree = new DoublyLinkedFiniteIntervalTree<IInterval<int>, int>();

                const int count = 10;

                var intervals = Enumerable.Range(0, count).Select(x => new IntervalBase<int>(x)).ToArray();

                foreach (var interval in intervals)
                    tree.Add(interval);
                foreach (var interval in intervals)
                    tree.Add(interval);

#if DEBUG
                Console.Out.WriteLine(tree.QuickGraph);
#endif
            }
            [Test]
            public void Similar()
            {
                var tree = new DoublyLinkedFiniteIntervalTree<IInterval<int>, int>();

                var intervals = new[]
                    {
                        new Interval(0),
                        new Interval(1),
                        new Interval(2),
                        new Interval(3),
                        new Interval(3, 4),
                        new Interval(4),
                        new Interval(4, 5),
                        new Interval(4, 5, IntervalType.Open),
                        new Interval(4, 5, IntervalType.HighIncluded),
                        new Interval(6),
                        new Interval(7),
                    };

                foreach (var interval in intervals)
                    tree.Add(interval);


                tree.FindOverlaps(4);

#if DEBUG
                Console.Out.WriteLine(tree.QuickGraph);
#endif
            }
        }
        */

        #region Force Add

        [TestFixture]
        class ForceAdd
        {
            private int callCount = 0;
            private Func<IntervalBase<int>, IntervalBase<int>, bool> action;

            [SetUp]
            public void SetUp()
            {
                callCount = 0;

                action = (x, y) =>
                {
                    callCount++;

                    y.SetEndpoints(x.High, x.High + y.High - y.Low);

                    return false;
                };
            }

            [Test]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ForceAdd_FaultyAction_Exception()
            {
                Func<Interval, Interval, bool> faultyAction = (x, y) => { return false; };

                var intervals = new[]
                {
                    new Interval(0, 2),
                    new Interval(2, 4),
                };
                var collection = new DoublyLinkedFiniteIntervalTree<Interval, int>(intervals);
                var interval = new Interval(1, 3);

                collection.ForceAdd(interval, faultyAction);
            }

            [Test]
            public void ForceAdd_InsertInGap_Nothing()
            {
                var intervals = new[]
                {
                    new Interval(0, 1),
                    new Interval(2, 3),
                };
                var collection = new DoublyLinkedFiniteIntervalTree<Interval, int>(intervals);
                var interval = new Interval(1, 2);

                Assert.That(collection.ForceAdd(interval, action), Is.False);
                Assert.That(callCount, Is.EqualTo(0));
            }

            [Test]
            public void ForceAdd_InsertOverlapWithGap_ShiftOnce()
            {
                var intervals = new[]
                {
                    new Interval(0, 2),
                    new Interval(3, 4),
                };
                var collection = new DoublyLinkedFiniteIntervalTree<Interval, int>(intervals);
                var interval = new Interval(1, 2);

                Assert.That(collection.ForceAdd(interval, action), Is.True);
                Assert.That(interval.IntervalEquals(new Interval(2, 3)));
                Assert.That(callCount, Is.EqualTo(1));
            }

            //[Test]
            //public void ForceAdd_InsertAsOverlapping_ShiftOverlapOnce()
            //{
            //    var intervals = new[]
            //    {
            //        new Interval(0, 2),
            //        new Interval(3, 4),
            //    };
            //    var collection = new DoublyLinkedFiniteIntervalTree<Interval, int>(intervals);
            //    var interval = new Interval(1, 2);

            //    Assert.That(collection.ForceAdd(interval, action), Is.True);
            //    Assert.That(interval.IntervalEquals(new Interval(2, 3)));
            //    Assert.That(callCount, Is.EqualTo(1));
            //}

            [Test]
            public void ForceAdd_InsertWithSameLow_ShiftOverlapOnce()
            {
                var intervals = new[]
                {
                    new Interval(0, 1),
                    new Interval(1, 2),
                    new Interval(3, 4),
                };
                var collection = new DoublyLinkedFiniteIntervalTree<Interval, int>(intervals);
                var interval = new Interval(1, 2);

                Assert.That(collection.ForceAdd(interval, action), Is.True);
                Assert.That(interval, Is.EqualTo(collection.Skip(2).First()));
                Assert.That(callCount, Is.EqualTo(1));
            }

            [Test]
            public void ForceAdd_InsertAsOverlappingStart_ShiftEverything()
            {
                var intervals = new[]
                {
                    new Interval(1, 2),
                    new Interval(2, 3),
                    new Interval(3, 4),
                };
                var collection = new DoublyLinkedFiniteIntervalTree<Interval, int>(intervals);
                var interval = new Interval(0, 2);

                Assert.That(collection.ForceAdd(interval, action), Is.True);
                Assert.That(collection.Count, Is.EqualTo(4));
                Assert.That(collection.Span.IntervalEquals(new Interval(0, 5)));
                Assert.That(callCount, Is.EqualTo(3));
            }

            [Test]
            public void ForceAdd_InsertAsOverlapping_ShiftUntilGap()
            {
                var intervals = new[]
                {
                    new Interval(0, 2),
                    new Interval(2, 3),
                    new Interval(3, 4),
                    new Interval(5, 6),
                };
                var collection = new DoublyLinkedFiniteIntervalTree<Interval, int>(intervals);
                var interval = new Interval(1, 2);

                Assert.That(collection.ForceAdd(interval, action), Is.True);
                Assert.That(collection.Count, Is.EqualTo(5));
                Assert.That(collection.Span.IntervalEquals(new Interval(0, 6)));
                Assert.That(callCount, Is.EqualTo(3));
            }

            [Test]
            public void ForceAdd_MeetingIntervals_ShiftAllOnce()
            {
                var count = 10;
                var intervals = new Interval[count];
                for (var i = 0; i < count; i++)
                    intervals[i] = new Interval(i * 2, i * 2 + 2);
                var collection = new DoublyLinkedFiniteIntervalTree<Interval, int>(intervals);

                var interval = new Interval(1, 2);
                Assert.That(collection.ForceAdd(interval, action), Is.True);
                foreach (var x in collection.Skip(2))
                    Assert.That(x.Low % 2 == 1);
            }

            //[Test]
            public void ForceAdd_PerformanceTestInteger()
            {
                var count = 1000 * 1000;
                var intervals = new Interval[count];
                for (var i = 0; i < count; i++)
                    intervals[i] = new Interval(i * 2, i * 2 + 2);
                var collection = new DoublyLinkedFiniteIntervalTree<Interval, int>(intervals);
                var interval = new Interval(count, count + 1);

                var sw = Stopwatch.StartNew();
                collection.ForceAdd(interval, action);
                var time = sw.ElapsedMilliseconds;
                Console.Out.WriteLine("ForceAdd with {0} intervals, doing {1} shifts, in {2} ms.", count, callCount, time);
            }

            //[Test]
            public void ForceAdd_PerformanceTestDateTime()
            {
                var count = 1000 * 1000;
                var intervals = new IntervalBase<DateTime>[count];
                var now = DateTime.Now.Date;
                for (var i = 0; i < count; i++)
                    intervals[i] = new IntervalBase<DateTime>(now.AddDays(i * 2), now.AddDays(i * 2 + 2));
                var collection = new DoublyLinkedFiniteIntervalTree<IntervalBase<DateTime>, DateTime>(intervals);
                var interval = new IntervalBase<DateTime>(now.AddDays(count), now.AddDays(count + 1));
                Func<IntervalBase<DateTime>, IntervalBase<DateTime>, bool> datetimeAction = (x, y) =>
                {
                    callCount++;
                    y.SetEndpoints(x.High, x.High + (y.High - y.Low));
                    return false;
                };

                var sw = Stopwatch.StartNew();
                collection.ForceAdd(interval, datetimeAction);
                var time = sw.ElapsedMilliseconds;
                Console.Out.WriteLine("ForceAdd with {0} intervals, doing {1} shifts, in {2} ms.", count, callCount, time);
            }
        }

        #endregion

        #endregion
    }
}
