using System;
using System.Linq;
using C5.Intervals;
using NUnit.Framework;

namespace C5.Intervals.Tests
{
    namespace IntervalBinarySearchTreeOld
    {
        #region Black-box

        class IntervalBinarySearchTreeOldTester_BlackBox : IntervalCollectionTester
        {
            protected override Type GetCollectionType()
            {
                return typeof(IntervalBinarySearchTreeOld<,>);
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

        // TODO: Hardcode examples from articles

        [TestFixture]
        class IntervalBinarySearchTreeOldTester_WhiteBox
        {
            #region Helpers
            // TODO: Builder pattern

            private static IntervalBinarySearchTreeOld<IInterval<int>, int> _createIBS(params IInterval<int>[] intervals)
            {
                return new IntervalBinarySearchTreeOld<IInterval<int>, int>(intervals);
            }

            private static IInterval<int>[] Ø
            {
                get { return new IInterval<int>[0]; }
            }

            private static IInterval<int> A
            {
                get { return new IntervalBase<int>(1, 3); }
            }

            private static IInterval<int>[] B
            {
                get
                {
                    return new IInterval<int>[] { new IntervalBase<int>(1, 3), new IntervalBase<int>(3, 5) };
                }
            }

            private static IInterval<int>[] C
            {
                get
                {
                    return new IInterval<int>[] { new IntervalBase<int>(1, 3), new IntervalBase<int>(2, 5), new IntervalBase<int>(3, 5) };
                }
            }

            private static IInterval<int>[] D
            {
                get
                {
                    return new IInterval<int>[] { new IntervalBase<int>(1, 3), new IntervalBase<int>(1, 5), new IntervalBase<int>(3, 5) };
                }
            }

            private static IInterval<int>[] E
            {
                get
                {
                    return new IInterval<int>[] { new IntervalBase<int>(1, 3), new IntervalBase<int>(1, 5) };
                }
            }

            private static IInterval<int>[] F
            {
                get
                {
                    var intervals = new ArrayList<IInterval<int>>();
                    for (int i = 1; i < 7 + 1; i++)
                    {
                        // Create [i;i]..[7;7] intervals
                        intervals.Add(new IntervalBase<int>(i, i, true, true));

                        // Create (i;i+1)..(6;7)
                        if (i > 1) intervals.Add(new IntervalBase<int>(i - 1, i, false));

                        // Create (2:4) & (4:6)
                        if (i % 2 == 0 && i + 2 < 7) intervals.Add(new IntervalBase<int>(i, i + 2, false));
                    }
                    return intervals.ToArray();
                }
            }

            private static IInterval<int>[] G
            {
                get
                {
                    return new IInterval<int>[]
                    {
                        new IntervalBase<int>(4, 4, true, true), new IntervalBase<int>(6, 6, true, true),
                        new IntervalBase<int>(1, 2), new IntervalBase<int>(5, 7, true, true),
                        new IntervalBase<int>(3, 3, true, true)
                    };
                }
            }


            #endregion

            #region Inner Classes

            #region Node

            #region Constructor

            // TODO: Test Node(T)

            #endregion

            #region Public Methods

            #region Intervals
            #endregion

            #region UpdateMaximumDepth
            #endregion

            #region CompareTo

            // TODO: Null values?

            #endregion

            #region ToString
            #endregion

            #region Swap
            #endregion

            #endregion

            #endregion

            #region Interval Set

            #region Constructor

            #region Set
            #endregion

            #region Empty
            #endregion

            #endregion

            #region ToString
            #endregion

            #region Minus Operator
            #endregion

            #endregion

            #endregion

            #region Constructors

            #region Non-empty Constructor and Privates
            #endregion

            #region Empty
            #endregion

            #endregion

            #region Collection Value

            #region IsEmpty
            #endregion

            #region Count
            #endregion

            #region CountSpeed
            #endregion

            #region Choose
            #endregion

            #endregion

            #region Enumerable

            #region GetEnumerator and Privates
            #endregion

            #endregion

            #region Events
            #endregion

            #region Interval Collection

            #region Properties

            #region Span

            #endregion

            #region Maximum Depth

            #endregion

            #region Allows Reference Duplicates
            #endregion

            #endregion

            #region Find Overlaps

            #region Stabbing

            [Test]
            public void FindOverlapsStabbing_WhileRootNotNull_Zero()
            {
                var inputDataSet = _createIBS(Ø);
                const int inputContents = 1;
                CollectionAssert.IsEmpty(inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsStabbing_WhileRootNotNull_One()
            {
                var inputDataSet = _createIBS(A);
                var inputContents = A.Low;
                var result = new[] { A };
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsStabbing_WhileRootNotNull_Many1()
            {
                var inputDataSet = _createIBS(B);
                const int inputContents = 2;
                var result = new[] { B.First() };
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsStabbing_WhileRootNotNull_Many2()
            {
                var inputDataSet = _createIBS(B);
                const int inputContents = 4;
                var result = new[] { B.Last() };
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsStabbing_RootLessIsEmpty()
            {
                var inputDataSet = _createIBS(B);
                var interval = new IntervalBase<int>(5, 5, true, true);
                inputDataSet.Add(interval);
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(3)).First());
                const int inputContents = 4;
                CollectionAssert.IsEmpty(inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsStabbing_RootGreaterIsEmpty()
            {
                var inputDataSet = _createIBS(B);
                var interval = new IntervalBase<int>(1, 1, true, true);
                inputDataSet.Add(interval);
                inputDataSet.Remove(inputDataSet.Filter(i => i.High.Equals(3)).First());
                const int inputContents = 2;
                CollectionAssert.IsEmpty(inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsStabbing_RootEqualIsEmpty()
            {
                var inputDataSet = _createIBS(B);
                var interval = new IntervalBase<int>(5, 5, true, true);
                inputDataSet.Add(interval);
                inputDataSet.Remove(interval);
                const int inputContents = 5;
                CollectionAssert.IsEmpty(inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsStabbing_ForeachIntervalInRootLessMany()
            {
                var inputDataSet = _createIBS(C);
                const int inputContents = 4;
                var result = _createIBS(C);
                result.Remove(result.First());
                var expected = inputDataSet.Where(x => x.Overlaps(inputContents));
                CollectionAssert.AreEquivalent(expected, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsStabbing_ForeachIntervalInRootGreaterOne()
            {
                var inputDataSet = _createIBS(B);
                const int inputContents = 2;
                var result = _createIBS(B);
                result.Remove(result.Last());
                var expected = inputDataSet.Where(x => x.Overlaps(inputContents));
                CollectionAssert.AreEquivalent(expected, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsStabbing_ForeachIntervalInRootGreaterMany()
            {
                var inputDataSet = _createIBS(D);
                const int inputContents = 2;
                var result = _createIBS(D);
                result.Remove(result.Last());
                var expected = inputDataSet.Where(x => x.Overlaps(inputContents));
                CollectionAssert.AreEquivalent(expected, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsStabbing_ForeachIntervalInRootequalMany()
            {
                var inputDataSet = _createIBS(E);
                const int inputContents = 1;
                var result = E;
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            #endregion

            #region Range

            [Test]
            public void FindOverlapsRange_IsEmpty()
            {
                var inputDataSet = _createIBS(Ø);
                var inputContents = new IntervalBase<int>(1, 2);
                CollectionAssert.IsEmpty(inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_NotSpanOverlaps()
            {
                var inputDataSet = _createIBS(A);
                var inputContents = new IntervalBase<int>(3, 4);
                CollectionAssert.IsEmpty(inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_NoSplitNode()
            {
                var inputDataSet = _createIBS(G);
                var inputContents = new IntervalBase<int>(1, 3, false, true);
                CollectionAssert.IsNotEmpty(inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_SpanOverlaps()
            {
                var inputDataSet = _createIBS(A);
                var inputContents = new IntervalBase<int>(1, 2);
                var result = new[] { A };
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }


            [Test]
            public void FindOverlapsRange_SpanMultipleOverlaps()
            {
                var inputDataSet = _createIBS(C);
                var inputContents = new IntervalBase<int>(1, 5);
                var result = C;
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootEqualIsEmpty()
            {
                var inputDataSet = _createIBS(B);
                var interval = new IntervalBase<int>(5, 5, true, true);
                inputDataSet.Add(interval);
                inputDataSet.Remove(interval);
                var inputContents = new IntervalBase<int>(4, 5, false, true);
                var result = new[] { B.Last() };
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootLessIsEmpty()
            {
                var inputDataSet = _createIBS(B);
                var interval = new IntervalBase<int>(5, 5, true, true);
                var interval2 = new IntervalBase<int>(5, 5, true, true);
                inputDataSet.Add(interval);
                inputDataSet.Add(interval2);
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(3)).First());
                var inputContents = new IntervalBase<int>(4, 5, false, true);
                var result = new[] { interval, interval2 };
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootLessIsEmpty2()
            {
                var inputDataSet = _createIBS(B);
                var interval = new IntervalBase<int>(5, 5, true, true);
                inputDataSet.Add(interval);
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(3)).First());
                var inputContents = new IntervalBase<int>(4, 4, true, true);
                CollectionAssert.IsEmpty(inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootEqualIsEmpty2()
            {
                var inputDataSet = _createIBS(B);
                var interval = new IntervalBase<int>(3, 5);
                inputDataSet.Add(interval);
                var inputContents = new IntervalBase<int>(4, 4, true, true);
                var expected = inputDataSet.Where(x => x.Overlaps(inputContents));
                CollectionAssert.AreEquivalent(expected, inputDataSet.FindOverlaps(inputContents));
            }


            [Test]
            public void FindOverlapsRange_RootEqualIsEmpty3()
            {
                var inputDataSet = _createIBS(B);
                var inputContents = new IntervalBase<int>(2, 4, true, true);
                var result = new[] { B.First(), B.Last() };
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootGreaterIsNotEmpty()
            {
                var inputDataSet = _createIBS(B);
                var inputContents = new IntervalBase<int>(1, 1, true, true);
                var result = new[] { B.First() };
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootGreaterIsNotEmpty2()
            {
                var inputDataSet = _createIBS(B);
                var interval = new IntervalBase<int>(1, 3, false);
                inputDataSet.Add(interval);
                var inputContents = new IntervalBase<int>(1, 1, true, true);
                var expected = inputDataSet.Where(x => x.Overlaps(inputContents));
                CollectionAssert.AreEquivalent(expected, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootGreaterIsEmpty()
            {
                var inputDataSet = _createIBS(B);
                var interval = new IntervalBase<int>(1, 1, true, true);
                inputDataSet.Add(interval);
                inputDataSet.Remove(inputDataSet.Filter(i => i.High.Equals(3)).First());
                var inputContents = new IntervalBase<int>(2, 2, true, true);
                CollectionAssert.IsEmpty(inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootGreaterIsEmpty2()
            {
                var inputDataSet = _createIBS(B);
                var interval = new IntervalBase<int>(1, 1, true, true);
                inputDataSet.Add(interval);
                inputDataSet.Remove(inputDataSet.Filter(i => i.High.Equals(3)).First());
                var inputContents = new IntervalBase<int>(1, 1, true, true);
                var result = new[] { interval };
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootGreaterOneIteration()
            {
                var inputDataSet = _createIBS(B);
                var inputContents = new IntervalBase<int>(2, 2, true, true);
                var result = new[] { B.First() };
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootGreaterManyIterations()
            {
                var inputDataSet = _createIBS(B);
                var interval = new IntervalBase<int>(1, 3, false);
                inputDataSet.Add(interval);
                var inputContents = new IntervalBase<int>(2, 2, true, true);
                var result = new[] { interval, B.First() };
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RangeLow()
            {
                var inputDataSet = _createIBS(F);
                var inputContents = new IntervalBase<int>(1, 7, true, true);
                CollectionAssert.AreEquivalent(inputDataSet, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RangeLow2()
            {
                var inputDataSet = _createIBS(B);
                var inputContents = new IntervalBase<int>(1, 3);
                var result = new[] { B.First() };
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RangeLow3()
            {
                var inputDataSet = _createIBS(B);
                inputDataSet.Remove(inputDataSet.First());
                var inputContents = new IntervalBase<int>(2, 4);
                var expected = inputDataSet.Where(x => x.Overlaps(inputContents));
                CollectionAssert.AreEquivalent(expected, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_SplitRightOnce()
            {
                var inputDataSet = _createIBS(F);
                var inputContents = new IntervalBase<int>(5, 7, false);
                var result = inputDataSet.Filter(i => i.Overlaps(inputContents));
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }


            #endregion

            #endregion

            #region Find Overlap

            #region Stabbing
            #endregion

            #region Range
            #endregion

            #endregion

            #region Count Overlaps

            #region Stabbing
            #endregion

            #region Range
            #endregion

            #endregion

            #region Extensible

            #region Is Read Only
            #endregion

            #region Add

            #region Events
            #endregion

            #endregion

            #region Add All

            #region Events
            #endregion

            #endregion

            #region Remove

            #region Events
            #endregion

            #endregion

            #region Clear

            #region Events
            #endregion

            #endregion

            #endregion

            #endregion

        }

        [TestFixture]
        [Category("Former Bug")]
        internal class FormerBugs
        {
            [Test]
            public void Span_CachingSpan()
            {
                var interval = new IntervalBase<int>(1, 2);

                var collection = new IntervalBinarySearchTreeOld<IntervalBase<int>, int>
                    {
                        interval,
                        new IntervalBase<int>(3,4),
                    };

                collection.Remove(interval);
                collection.Add(interval);
                var span = collection.Span;
                Assert.Pass();
            }

            [Test]
            public void Span_RetrievSpanFromNodeWithEmptyIntervalSets()
            {
                var collection = new IntervalBinarySearchTreeOld<IntervalBase<int>, int>
                    {
                        new IntervalBase<int>(3,4, IntervalType.HighIncluded)
                    };
                var span = collection.Span;
                Assert.Pass();
            }
        }

        #endregion
    }
}
