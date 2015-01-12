using System;
using System.Linq;
using NUnit.Framework;

namespace C5.Intervals.Tests
{
    namespace IntervalBinarySearchTree
    {
        #region Black-box

        abstract class IntervalBinarySearchTreeTester_BlackBox : IntervalCollectionTester
        {
            protected override Type GetCollectionType()
            {
                return typeof(IntervalBinarySearchTree<,>);
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

        class IntervalBinarySearchTreeTesterPreConstructBlackBox : IntervalBinarySearchTreeTester_BlackBox
        {
            protected override object[] AdditionalParameters()
            {
                return new object[] { true };
            }
        }

        class IntervalBinarySearchTreeTesterBlackBox : IntervalBinarySearchTreeTester_BlackBox
        {
            protected override object[] AdditionalParameters()
            {
                return new object[] { false };
            }
        }

        #endregion

        #region White-box

        // TODO: Hardcode examples from articles

        [TestFixture]
        class IntervalBinarySearchTreeTester_WhiteBox
        {
            #region Helpers
            // TODO: Builder pattern

            private static IntervalBinarySearchTree<IInterval<int>, int> _createIBS(params IInterval<int>[] intervals)
            {
                return new IntervalBinarySearchTree<IInterval<int>, int>(intervals);
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
                    for (var i = 1; i < 7 + 1; i++)
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
            private static IInterval<int>[] H
            {
                get
                {
                    return new IInterval<int>[] { new IntervalBase<int>(1, 1, true, true), new IntervalBase<int>(3, 5) };
                }
            }

            private static IInterval<int>[] J
            {
                get
                {
                    var intervals = new ArrayList<IInterval<int>>();
                    for (var i = 1; i < 7 + 1; i++)
                    {
                        // Create [i;i]..[7;7] intervals
                        if (i != 6) // Don't create [6;6]
                            intervals.Add(new IntervalBase<int>(i, i, true, true));

                        // Create (i;i+1)..(3;4)
                        if (i > 1 && i < 5) intervals.Add(new IntervalBase<int>(i - 1, i, false));

                        // Create (2:4) & (4:6)
                        if (i % 2 == 0 && i + 2 < 7) intervals.Add(new IntervalBase<int>(i, i + 2, false));
                    }
                    return intervals.ToArray();
                }
            }

            private static IInterval<int>[] K
            {
                get
                {
                    var intervals = new ArrayList<IInterval<int>>();
                    for (var i = 1; i < 7 + 1; i++)
                    {
                        // Create [i;i]..[7;7] intervals
                        intervals.Add(new IntervalBase<int>(i, i, true, true));

                        // Create (i;i+1)..(6;7)
                        if (i > 2) intervals.Add(new IntervalBase<int>(i - 1, i, false));

                        // Create (2:4) & (4:6)
                        if (i % 2 == 0 && i + 2 < 7) intervals.Add(new IntervalBase<int>(i, i + 2, false));
                    }
                    return intervals.ToArray();
                }
            }

            private static IInterval<int>[] L
            {
                get
                {
                    var intervals = new ArrayList<IInterval<int>>();
                    for (var i = 1; i < 7 + 1; i++)
                    {
                        // Create [i;i]..[7;7] intervals, except [5;5]
                        if (i != 5)
                            intervals.Add(new IntervalBase<int>(i, i, true, true));

                        // Create (i;i+1)..(6;7)
                        if (i > 1) intervals.Add(new IntervalBase<int>(i - 1, i, false));

                        // Create (2:4) & (4:6)
                        if (i % 2 == 0 && i + 2 < 7) intervals.Add(new IntervalBase<int>(i, i + 2, false));
                    }
                    return intervals.ToArray();
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
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsStabbing_ForeachIntervalInRootGreaterOne()
            {
                var inputDataSet = _createIBS(B);
                const int inputContents = 2;
                var result = _createIBS(B);
                result.Remove(result.Last());
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsStabbing_ForeachIntervalInRootGreaterMany()
            {
                var inputDataSet = _createIBS(D);
                const int inputContents = 2;
                var result = _createIBS(D);
                result.Remove(result.Last());
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
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
                var result = new[] { B.Last(), interval };
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
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
                var result = new[] { B.Last() };
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_SplitRightOnce()
            {
                var inputDataSet = _createIBS(F);
                var inputContents = new IntervalBase<int>(5, 7, false);
                var result = inputDataSet.Filter(i => i.Overlaps(inputContents));
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootGreaterIsEmpty3()
            {
                var inputDataSet = _createIBS(B);
                var interval = new IntervalBase<int>(1, 1, true, true);
                inputDataSet.Add(interval);
                inputDataSet.Remove(inputDataSet.Filter(i => i.High.Equals(3)).First());
                var inputContents = new IntervalBase<int>(2, 3);
                CollectionAssert.IsEmpty(inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootGreaterIsNull()
            {
                var inputDataSet = _createIBS(H);
                var inputContents = new IntervalBase<int>(2, 3);
                CollectionAssert.IsEmpty(inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootGreaterIsNotEmpty3()
            {
                var inputDataSet = _createIBS(B);
                var interval = new IntervalBase<int>(1, 3, false);
                inputDataSet.Add(interval);
                var inputContents = new IntervalBase<int>(2, 3);
                var expected = inputDataSet.Where(x => x.Overlaps(inputContents));
                CollectionAssert.AreEquivalent(expected, inputDataSet.FindOverlaps(inputContents));
            }


            [Test]
            public void FindOverlapsRange_RootEqualEmpty()
            {
                var inputDataSet = _createIBS(F);
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(2) && i.High.Equals(2)).First());
                var inputContents = new IntervalBase<int>(1, 7, true, true);
                CollectionAssert.AreEquivalent(inputDataSet, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootEqualNotEmpty()
            {
                var inputDataSet = _createIBS(F);
                inputDataSet.Add(new IntervalBase<int>(2, 2, true, true));
                var inputContents = new IntervalBase<int>(1, 7, true, true);
                CollectionAssert.AreEquivalent(inputDataSet, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootGreaterEmpty()
            {
                var inputDataSet = _createIBS(F);
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(2) && i.High.Equals(4)).First());
                var inputContents = new IntervalBase<int>(1, 7, true, true);
                CollectionAssert.AreEquivalent(inputDataSet, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootGreaterNotEmpty()
            {
                var inputDataSet = _createIBS(F);
                inputDataSet.Add(new IntervalBase<int>(2, 4, false));
                var inputContents = new IntervalBase<int>(1, 7, true, true);
                CollectionAssert.AreEquivalent(inputDataSet, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_SplitRightOnce2()
            {
                var inputDataSet = _createIBS(F);
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(5) && i.High.Equals(6)).First());
                var inputContents = new IntervalBase<int>(5, 7, false);
                var result = inputDataSet.Filter(i => i.Overlaps(inputContents));
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_SplitRightOnce3()
            {
                var inputDataSet = _createIBS(F);
                inputDataSet.Add(new IntervalBase<int>(5, 6, false));
                var inputContents = new IntervalBase<int>(5, 7, false);
                var result = inputDataSet.Filter(i => i.Overlaps(inputContents));
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootEqualsMultiple()
            {
                var inputDataSet = _createIBS(F);
                inputDataSet.Add(new IntervalBase<int>(1, 1, true, true));
                var inputContents = new IntervalBase<int>(1, 7, true, true);
                CollectionAssert.AreEquivalent(inputDataSet, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootLessMultiple()
            {
                var inputDataSet = _createIBS(B);
                var newInterval = new IntervalBase<int>(3, 5);
                inputDataSet.Add(newInterval);
                var inputContents = new IntervalBase<int>(2, 4);
                var result = new[] { newInterval, B.Last(), B.First() };
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootLessNull()
            {
                var inputDataSet = _createIBS(B);
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(3) && i.High.Equals(5)).First());
                inputDataSet.Add(new IntervalBase<int>(5, 5, true, true));
                var inputContents = new IntervalBase<int>(2, 4);
                var result = new[] { B.First() };
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootLessZero()
            {
                var inputDataSet = _createIBS(B);
                inputDataSet.Add(new IntervalBase<int>(5, 5, true, true));
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(3) && i.High.Equals(5)).First());
                var inputContents = new IntervalBase<int>(2, 4);
                var result = new[] { B.First() };
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootMultiples()
            {
                var inputDataSet = _createIBS(F);
                inputDataSet.Add(new IntervalBase<int>(6, 7, false));
                inputDataSet.Add(new IntervalBase<int>(7, 7, true, true));
                var inputContents = new IntervalBase<int>(1, 7, true, true);
                CollectionAssert.AreEquivalent(inputDataSet, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootLessIsEmpty1()
            {
                var inputDataSet = _createIBS(F);
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(4) && i.High.Equals(6)).First());
                var inputContents = new IntervalBase<int>(1, 7, true, true);
                CollectionAssert.AreEquivalent(inputDataSet, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootEqualIsEmpty1()
            {
                var inputDataSet = _createIBS(F);
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(6) && i.High.Equals(6)).First());
                var inputContents = new IntervalBase<int>(1, 7, true, true);
                CollectionAssert.AreEquivalent(inputDataSet, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootLeftIsNotEmpty()
            {
                var inputDataSet = _createIBS(F);
                var inputContents = new IntervalBase<int>(1, 6, true, true);
                var result = _createIBS(F);
                result.Remove(result.Filter(i => i.Low.Equals(7) && i.High.Equals(7)).First());
                result.Remove(result.Filter(i => i.Low.Equals(6) && i.High.Equals(7)).First());
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }


            [Test]
            public void FindOverlapsRange_RootLeftIsNotEmpty2()
            {
                var inputDataSet = _createIBS(F);
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(4) && i.High.Equals(6)).First());
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(6) && i.High.Equals(6)).First());
                var inputContents = new IntervalBase<int>(1, 6, true, true);
                var result = _createIBS(F);
                result.Remove(result.Filter(i => i.Low.Equals(7) && i.High.Equals(7)).First());
                result.Remove(result.Filter(i => i.Low.Equals(6) && i.High.Equals(7)).First());
                result.Remove(result.Filter(i => i.Low.Equals(6) && i.High.Equals(6)).First());
                result.Remove(result.Filter(i => i.Low.Equals(4) && i.High.Equals(6)).First());
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootEqualIsNull()
            {
                var inputDataSet = _createIBS(J);
                var inputContents = new IntervalBase<int>(1, 6, true, true);
                var result = _createIBS(F);
                result.Remove(result.Filter(i => i.Low.Equals(7) && i.High.Equals(7)).First());
                result.Remove(result.Filter(i => i.Low.Equals(6) && i.High.Equals(7)).First());
                result.Remove(result.Filter(i => i.Low.Equals(6) && i.High.Equals(6)).First());
                result.Remove(result.Filter(i => i.Low.Equals(5) && i.High.Equals(6)).First());
                result.Remove(result.Filter(i => i.Low.Equals(4) && i.High.Equals(5)).First());
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootLessIsNull()
            {
                var inputDataSet = _createIBS(J);
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(5) && i.High.Equals(5)).First());
                var inputContents = new IntervalBase<int>(1, 6, true, true);
                var result = _createIBS(F);
                result.Remove(result.Filter(i => i.Low.Equals(7) && i.High.Equals(7)).First());
                result.Remove(result.Filter(i => i.Low.Equals(6) && i.High.Equals(7)).First());
                result.Remove(result.Filter(i => i.Low.Equals(6) && i.High.Equals(6)).First());
                result.Remove(result.Filter(i => i.Low.Equals(5) && i.High.Equals(6)).First());
                result.Remove(result.Filter(i => i.Low.Equals(5) && i.High.Equals(5)).First());
                result.Remove(result.Filter(i => i.Low.Equals(4) && i.High.Equals(5)).First());
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_IntervalsEndingInNodeOnce()
            {
                var inputDataSet = _createIBS(F);
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(2) && i.High.Equals(2)).First());
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(1) && i.High.Equals(2)).First());
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(2) && i.High.Equals(3)).First());
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(2) && i.High.Equals(4)).First());
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(3) && i.High.Equals(4)).First());
                var inputContents = new IntervalBase<int>(1, 7, true, true);
                CollectionAssert.AreEquivalent(inputDataSet, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootRightOnce()
            {
                var inputDataSet = _createIBS(F);
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(2) && i.High.Equals(3)).First());
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(3) && i.High.Equals(4)).First());
                var inputContents = new IntervalBase<int>(1, 7, true, true);
                CollectionAssert.AreEquivalent(inputDataSet, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootGreaterIsNull1()
            {
                var inputDataSet = _createIBS(K);
                var inputContents = new IntervalBase<int>(1, 7, true, true);
                CollectionAssert.AreEquivalent(inputDataSet, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_QueryLowEqualNotNull()
            {
                var inputDataSet = _createIBS(F);
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(5) && i.High.Equals(5)).First());
                var inputContents = new IntervalBase<int>(5, 7);
                var result = inputDataSet.Filter(i => i.Overlaps(inputContents));
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_QueryLowEqualIsNull()
            {
                var inputDataSet = _createIBS(L);
                var inputContents = new IntervalBase<int>(5, 7);
                var result = inputDataSet.Filter(i => i.Overlaps(inputContents));
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootRightMultiple()
            {
                var inputDataSet = _createIBS(F);
                var inputContents = new IntervalBase<int>(2, 7, true, true);
                var result = inputDataSet.Filter(i => i.Overlaps(inputContents));
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootRightOnce1()
            {
                var inputDataSet = _createIBS(F);
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(2) && i.High.Equals(3)).First());
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(3) && i.High.Equals(4)).First());
                var inputContents = new IntervalBase<int>(2, 7, true, true);
                var result = inputDataSet.Filter(i => i.Overlaps(inputContents));
                CollectionAssert.AreEquivalent(result, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_IntervalsEndingInNodeOnce1()
            {
                var inputDataSet = _createIBS(F);
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(4) && i.High.Equals(6)).First());
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(5) && i.High.Equals(6)).First());
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(6) && i.High.Equals(7)).First());
                var inputContents = new IntervalBase<int>(1, 7, true, true);
                CollectionAssert.AreEquivalent(inputDataSet, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootLeftOnce()
            {
                var inputDataSet = _createIBS(F);
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(4) && i.High.Equals(5)).First());
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(4) && i.High.Equals(6)).First());
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(5) && i.High.Equals(6)).First());
                inputDataSet.Remove(inputDataSet.Filter(i => i.Low.Equals(6) && i.High.Equals(7)).First());
                var inputContents = new IntervalBase<int>(1, 7, true, true);
                CollectionAssert.AreEquivalent(inputDataSet, inputDataSet.FindOverlaps(inputContents));
            }

            [Test]
            public void FindOverlapsRange_RootLessIsNull1()
            {
                var inputDataSet = _createIBS(J);
                var inputContents = new IntervalBase<int>(1, 7, true, true);
                CollectionAssert.AreEquivalent(inputDataSet, inputDataSet.FindOverlaps(inputContents));
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

                var collection = new IntervalBinarySearchTree<IntervalBase<int>, int>
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
                var collection = new IntervalBinarySearchTree<IntervalBase<int>, int>
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
