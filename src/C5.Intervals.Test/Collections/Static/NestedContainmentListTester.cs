using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace C5.Intervals.Tests
{
    namespace NestedContainmentList
    {
        #region Black-box

        class NestedContainmentListTester_BlackBox : IntervalCollectionTester
        {
            protected override Type GetCollectionType()
            {
                return typeof(NestedContainmentList<,>);
            }

            protected override Speed CountSpeed()
            {
                return Speed.Constant;
            }

            protected override bool AllowsReferenceDuplicates()
            {
                return true;
            }
        }

        #endregion

        #region White-box

        #region Statement testing

        [TestFixture]
        public class Statement
        {
            private IInterval<int> _overlap;

            // ReSharper disable InconsistentNaming
            private static readonly IInterval<int> A = new IntervalBase<int>(1, 5, true, true);
            private static readonly IInterval<int> B = new IntervalBase<int>(3, 8, true, true);
            private static readonly IInterval<int> C = new IntervalBase<int>(9, 17, true, true);
            private static readonly IInterval<int> D = new IntervalBase<int>(12, 20, true, true);
            private static readonly IInterval<int> E = new IntervalBase<int>(2, 7, true, true);
            private static readonly IInterval<int> F = new IntervalBase<int>(2, 16, true, true);
            private static readonly IInterval<int> G = new IntervalBase<int>(3, 8, true, true);
            private static readonly IInterval<int> H = new IntervalBase<int>(5, 12, true, true);
            private static readonly IInterval<int> I = new IntervalBase<int>(11, 17, true, true);
            private static readonly IInterval<int> M = new IntervalBase<int>(3, 15, true, true);
            private static readonly IInterval<int> N = new IntervalBase<int>(4, 6, true, true);
            private static readonly IInterval<int> O = new IntervalBase<int>(7, 12, true, true);
            private static readonly IInterval<int> P = new IntervalBase<int>(10, 11, true, true);
            private static readonly IInterval<int> Q = new IntervalBase<int>(13, 14, true, true);

            private static readonly IEnumerable<IInterval<int>> dataSetA = new[] { E };
            private static readonly IEnumerable<IInterval<int>> dataSetB = new[] { A, B, C, D };
            private static readonly IEnumerable<IInterval<int>> dataSetC = new[] { F, G, H, I };
            private static readonly IEnumerable<IInterval<int>> dataSetD = new[] { F, M, N, O, C, P, Q };
            private static readonly IEnumerable<IInterval<int>> dataSetE = new[] { B, D };
            private static readonly IEnumerable<IInterval<int>> dataSetF = new[] { A, F, G, H, I };
            private static readonly IEnumerable<IInterval<int>> dataSetG = new[] { F, G, I };

            #region Constructor

            [Test]
            public void Constructor_Empty()
            {
                CollectionAssert.AreEquivalent(Enumerable.Empty<IInterval<int>>(), new NestedContainmentList<IInterval<int>, int>(Enumerable.Empty<IInterval<int>>()));
            }

            [Test]
            public void Constructor_OneInterval()
            {
                CollectionAssert.AreEquivalent(dataSetA, new NestedContainmentList<IInterval<int>, int>(dataSetA));
            }

            [Test]
            public void Constructor_MoreThanOneIntervalAndOneContainmentLayer()
            {
                CollectionAssert.AreEquivalent(dataSetB, new NestedContainmentList<IInterval<int>, int>(dataSetB));
            }

            [Test]
            public void Constructor_MoreThanOneIntervalAndTwoContainmentLayers()
            {
                CollectionAssert.AreEquivalent(dataSetC, new NestedContainmentList<IInterval<int>, int>(dataSetC));
            }

            [Test]
            public void Constructor_MoreThanOneIntervalAndMoreThanTwoContainmentLayers()
            {
                CollectionAssert.AreEquivalent(dataSetD, new NestedContainmentList<IInterval<int>, int>(dataSetD));
            }

            [Test]
            public void Constructor_ThreeContainments()
            {
                var moreThanOne = new NestedContainmentList<IInterval<int>, int>(new[]
                    {
                        new IntervalBase<int>(1, 7, true, true),
                        new IntervalBase<int>(2, 6, true, true),
                        new IntervalBase<int>(3, 8, true, true),
                        new IntervalBase<int>(4, 5, true, true)
                    });
                CollectionAssert.AreEquivalent(new[] 
                    { 
                        new IntervalBase<int>(1, 7, true, true),
                        new IntervalBase<int>(2, 6, true, true),
                        new IntervalBase<int>(3, 8, true, true),
                        new IntervalBase<int>(4, 5, true, true) }, moreThanOne);
            }

            /*
            [Test]
            public void Constructor_FirstContainssecondEndEqual()
            {
                var moreThanOne = new NestedContainmentList<IInterval<int>, int>(new[] { J, L });
                CollectionAssert.AreEquivalent(new[] { J, L }, moreThanOne);
            }
            */

            #endregion

            #region CountOverlap

            [Test]
            public void CountOverlap_Empty()
            {
                Assert.AreEqual(0, new NestedContainmentList<IInterval<int>, int>(Enumerable.Empty<IInterval<int>>()).
                    CountOverlaps(new IntervalBase<int>(2, 7, true, true)));
            }

            [Test]
            public void CountOverlap_Empty_NullQuery()
            {
                //Assert.AreEqual(0, new NestedContainmentList<IInterval<int>, int>(Enumerable.Empty<IInterval<int>>()).CountOverlaps(null));
            }

            [Test]
            public void CountOverlap_OneInterval()
            {
                Assert.AreEqual(1, new NestedContainmentList<IInterval<int>, int>(dataSetA).
                    CountOverlaps(new IntervalBase<int>(2, 7, true, true)));
            }

            [Test]
            public void CountOverlap_OneInterval2()
            {
                Assert.AreEqual(1, new NestedContainmentList<IInterval<int>, int>(new[] { new IntervalBase<int>(2, 7, false, true) }).
                    CountOverlaps(new IntervalBase<int>(2, 7, true, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals3()
            {
                Assert.AreEqual(1, new NestedContainmentList<IInterval<int>, int>(new[]
                    {
                        new IntervalBase<int>(2, 7, true, true),
                        new IntervalBase<int>(1, 6, true, true),
                        new IntervalBase<int>(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalBase<int>(0, 1, true, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals4()
            {
                Assert.AreEqual(1, new NestedContainmentList<IInterval<int>, int>(new[]
                    {
                        new IntervalBase<int>(2, 7, false, true),
                        new IntervalBase<int>(0, 6, true, true),
                        new IntervalBase<int>(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalBase<int>(-1, 1)));
            }

            [Test]
            public void CountOverlap_MoreIntervals5()
            {
                Assert.AreEqual(1, new NestedContainmentList<IInterval<int>, int>(new[]
                    {
                        new IntervalBase<int>(2, 7, false, true),
                        new IntervalBase<int>(0, 6, true, true),
                        new IntervalBase<int>(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalBase<int>(-1, 1, true, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals6()
            {
                Assert.AreEqual(1, new NestedContainmentList<IInterval<int>, int>(new[]
                    {
                        new IntervalBase<int>(2, 7, true, true),
                        new IntervalBase<int>(0, 6, true, true),
                        new IntervalBase<int>(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalBase<int>(-1, 2)));
            }

            [Test]
            public void CountOverlap_MoreIntervals61()
            {
                Assert.AreEqual(1, new NestedContainmentList<IInterval<int>, int>(new[]
                    {
                        new IntervalBase<int>(2, 7, false, true),
                        new IntervalBase<int>(0, 6, true, true),
                        new IntervalBase<int>(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalBase<int>(-1, 2, true, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals7()
            {
                Assert.AreEqual(1, new NestedContainmentList<IInterval<int>, int>(new[]
                    {
                        new IntervalBase<int>(2, 7, false, true),
                        new IntervalBase<int>(0, 6, true, true),
                        new IntervalBase<int>(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalBase<int>(-1, 2)));
            }

            [Test]
            public void CountOverlap_MoreIntervals8()
            {
                Assert.AreEqual(1, new NestedContainmentList<IInterval<int>, int>(new[]
                    {
                        new IntervalBase<int>(2, 7, true, true),
                        new IntervalBase<int>(0, 6, true, true),
                        new IntervalBase<int>(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalBase<int>(-1, 1)));
            }

            // ******************

            [Test]
            public void CountOverlap_MoreIntervals9()
            {
                Assert.AreEqual(1, new NestedContainmentList<IInterval<int>, int>(new[]
                    {
                        new IntervalBase<int>(2, 7, true, true),
                        new IntervalBase<int>(1, 6, true, true),
                        new IntervalBase<int>(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalBase<int>(7, 8, true, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals10()
            {
                Assert.AreEqual(0, new NestedContainmentList<IInterval<int>, int>(new[]
                    {
                        new IntervalBase<int>(2, 7),
                        new IntervalBase<int>(1, 6, true, true),
                        new IntervalBase<int>(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalBase<int>(7, 8, true, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals11()
            {
                Assert.AreEqual(0, new NestedContainmentList<IInterval<int>, int>(new[]
                    {
                        new IntervalBase<int>(2, 7, true, true),
                        new IntervalBase<int>(1, 6, true, true),
                        new IntervalBase<int>(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalBase<int>(7, 8, false, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals12()
            {
                Assert.AreEqual(0, new NestedContainmentList<IInterval<int>, int>(new[]
                    {
                        new IntervalBase<int>(2, 7),
                        new IntervalBase<int>(1, 6, true, true),
                        new IntervalBase<int>(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalBase<int>(7, 8, false, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals13()
            {
                Assert.AreEqual(0, new NestedContainmentList<IInterval<int>, int>(new[]
                    {
                        new IntervalBase<int>(2, 7),
                        new IntervalBase<int>(1, 6, true, true),
                        new IntervalBase<int>(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalBase<int>(8, 9, false, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals14()
            {
                Assert.AreEqual(0, new NestedContainmentList<IInterval<int>, int>(new[]
                    {
                        new IntervalBase<int>(2, 7),
                        new IntervalBase<int>(1, 6, true, true),
                        new IntervalBase<int>(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalBase<int>(8, 9, true, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals15()
            {
                Assert.AreEqual(0, new NestedContainmentList<IInterval<int>, int>(new[]
                    {
                        new IntervalBase<int>(2, 7, true, true),
                        new IntervalBase<int>(1, 6, true, true),
                        new IntervalBase<int>(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalBase<int>(8, 9, false, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals16()
            {
                Assert.AreEqual(0, new NestedContainmentList<IInterval<int>, int>(new[]
                    {
                        new IntervalBase<int>(2, 7, true, true),
                        new IntervalBase<int>(1, 6, true, true),
                        new IntervalBase<int>(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalBase<int>(8, 9, true, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals17()
            {
                Assert.AreEqual(1, new NestedContainmentList<IInterval<int>, int>(new[]
                    {
                        new IntervalBase<int>(2, 7, true, true),
                        new IntervalBase<int>(1, 5, true, true),
                        new IntervalBase<int>(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalBase<int>(6, 9, true, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals18()
            {
                Assert.AreEqual(1, new NestedContainmentList<IInterval<int>, int>(new[]
                    {
                        new IntervalBase<int>(2, 7, true, true),
                        new IntervalBase<int>(1, 5, true, true),
                        new IntervalBase<int>(4, 5, true, true),
                        new IntervalBase<int>(0, 1, true, true),
                        new IntervalBase<int>(8, 12, true, true)
                    }).
                    CountOverlaps(new IntervalBase<int>(6, 7, true, true)));
            }

            [Test]
            public void CountOverlap_MoreThanOneInterval()
            {
                Assert.AreEqual(5, new NestedContainmentList<IInterval<int>, int>(dataSetD).
                    CountOverlaps(new IntervalBase<int>(6, 9, true, true)));
            }

            [Test]
            public void CountOverlap_ContainmentQueryHits()
            {
                Assert.AreEqual(3, new NestedContainmentList<IInterval<int>, int>(dataSetF).
                    CountOverlaps(new IntervalBase<int>(6, 7, true, true)));
            }

            [Test]
            public void CountOverlap_MoreThanOneIntervalQueryBefore()
            {
                Assert.AreEqual(0, new NestedContainmentList<IInterval<int>, int>(dataSetE).
                    CountOverlaps(new IntervalBase<int>(1, 2, true, true)));
            }

            [Test]
            public void CountOverlap_MoreThanOneIntervalQueryAfter()
            {
                Assert.AreEqual(0, new NestedContainmentList<IInterval<int>, int>(dataSetE).
                    CountOverlaps(new IntervalBase<int>(21, 23, true, true)));
            }

            #endregion

            #region FindOverlaps

            [Test]
            public void FindOverlapsStabbing_QueryZeroIntervals()
            {
                CollectionAssert.AreEquivalent(Enumerable.Empty<IInterval<int>>(), new NestedContainmentList<IInterval<int>, int>(Enumerable.Empty<IInterval<int>>()).FindOverlaps(2));
            }

            [Test]
            public void FindOverlapsRange_QueryZeroIntervals()
            {
                CollectionAssert.AreEquivalent(Enumerable.Empty<IInterval<int>>(), new NestedContainmentList<IInterval<int>, int>(Enumerable.Empty<IInterval<int>>()).FindOverlaps(new IntervalBase<int>(21, 23, true, true)));
            }

            [Test]
            public void FindOverlapsStabbing_QueryPointOneOrMoreIntervals()
            {
                CollectionAssert.AreEquivalent(new[] { A }, new NestedContainmentList<IInterval<int>, int>(dataSetB).FindOverlaps(2));
            }

            [Test]
            public void FindOverlaps_MoreThanOneIntervalQueryAfter()
            {
                Assert.AreEqual(Enumerable.Empty<IInterval<int>>(), new NestedContainmentList<IInterval<int>, int>(dataSetE).FindOverlaps(new IntervalBase<int>(21, 23, true, true)));
            }

            [Test]
            public void FindOverlaps_ContainmentQueryHits()
            {
                Assert.AreEqual(3, new NestedContainmentList<IInterval<int>, int>(dataSetF).FindOverlaps(new IntervalBase<int>(6, 7, true, true)).Count());
            }

            [Test]
            public void FindOverlaps_MoreThanOneIntervalQueryBefore()
            {
                Assert.AreEqual(Enumerable.Empty<IInterval<int>>(), new NestedContainmentList<IInterval<int>, int>(dataSetE).FindOverlaps(new IntervalBase<int>(1, 2, true, true)));
            }

            #endregion

            #region Enumerator

            [Test]
            public void Enumerator_Empty()
            {
                var result = new ArrayList<IInterval<int>>();
                var list = new NestedContainmentList<IInterval<int>, int>(Enumerable.Empty<IInterval<int>>());
                var enumerator = list.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    result.Add(enumerator.Current);
                }

                CollectionAssert.AreEquivalent(Enumerable.Empty<IInterval<int>>(), result);
            }

            [Test]
            public void Enumerator_OneInterval()
            {
                var result = new ArrayList<IInterval<int>>();
                var list = new NestedContainmentList<IInterval<int>, int>(dataSetA);
                var enumerator = list.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    result.Add(enumerator.Current);
                }

                CollectionAssert.AreEquivalent(dataSetA, result);
            }

            [Test]
            public void Enumerator_MoreContainments()
            {
                var result = new ArrayList<IInterval<int>>();
                var list = new NestedContainmentList<IInterval<int>, int>(dataSetC);
                var enumerator = list.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    result.Add(enumerator.Current);
                }

                CollectionAssert.AreEquivalent(dataSetC, result);
            }

            [Test]
            public void Enumerator_OneContainment()
            {
                var result = new ArrayList<IInterval<int>>();
                var list = new NestedContainmentList<IInterval<int>, int>(dataSetG);
                var enumerator = list.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    result.Add(enumerator.Current);
                }

                CollectionAssert.AreEquivalent(dataSetG, result);
            }

            #endregion

            #region Span

            [Test]
            public void Span_Empty()
            {
                //Assert.Throws<InvalidOperationException>(() => { var span = new NestedContainmentList<IInterval<int>, int>(Enumerable.Empty<IInterval<int>>()).Span; });
            }

            [Test]
            public void Span_MoreThanZero()
            {
                Assert.That(new NestedContainmentList<IInterval<int>, int>(dataSetB).Span.Equals(new IntervalBase<int>(1, 20, true, true)));
            }

            #endregion

            #region OverlapExists

            [Test]
            public void OverlapExists_Empty()
            {
                Assert.False(new NestedContainmentList<IInterval<int>, int>(Enumerable.Empty<IInterval<int>>()).
                    FindOverlap(new IntervalBase<int>(4, 5, true, true), out _overlap));
            }

            [Test]
            public void OverlapExists_QueryOutOfSpan()
            {
                Assert.False(new NestedContainmentList<IInterval<int>, int>(dataSetB).FindOverlap(new IntervalBase<int>(21, 23, true, true), out _overlap));
            }

            [Test]
            public void OverlapExists_Hit()
            {
                Assert.True(new NestedContainmentList<IInterval<int>, int>(dataSetB).FindOverlap(new IntervalBase<int>(4, 5, true, true), out _overlap));
            }

            [Test]
            public void OverlapExists_NoHit()
            {
                Assert.False(new NestedContainmentList<IInterval<int>, int>(dataSetE).FindOverlap(new IntervalBase<int>(9, 11, true, true), out _overlap));
            }

            #endregion

            [Test]
            public void CountSpeed_NotEmpty()
            {
                Assert.That(new NestedContainmentList<IInterval<int>, int>(dataSetA).CountSpeed.Equals(Speed.Constant));
            }

            [Test]
            public void Choose_Empty()
            {
                Assert.Throws<NoSuchItemException>(() =>
                { new NestedContainmentList<IInterval<int>, int>(Enumerable.Empty<IInterval<int>>()).Choose(); });
            }

            [Test]
            public void Choose_NotEmpty()
            {
                Assert.NotNull(new NestedContainmentList<IInterval<int>, int>(dataSetA).Choose());
            }

            [Test]
            public void ToString_Null()
            {
                var list = new NestedContainmentList<IInterval<int>, int>(Enumerable.Empty<IInterval<int>>());
                Assert.AreEqual("{  }", list.ToString());
            }

            [Test]
            public void ToString_NotNull()
            {
                var list = new NestedContainmentList<IInterval<int>, int>(dataSetA);
                Assert.AreEqual("{ [2:7] }", list.ToString());
            }

        }

        #endregion

        [TestFixture]
        [Category("Former Bug")]
        class FormerBugs
        {
            [Test]
            [Category("Former Bug")]
            public void Constructor_DoubleContainedInterval_MovedToNewLayer()
            {
                var intervals = new[]
                {
                    new IntervalBase<int>(0, 10),
                    new IntervalBase<int>(1, 8),
                    new IntervalBase<int>(2, 6),
                    new IntervalBase<int>(3, 9),
                    new IntervalBase<int>(4, 5),
                };

                var collection = new NestedContainmentList<IInterval<int>, int>(intervals);

                //TODO: Expand beyond simple construction?
            }
        }

        #endregion
    }
}
