using System;
using NUnit.Framework;

namespace C5.Intervals.Tests
{
    using Interval = IntervalBase<int>;

    namespace StaticIntervalTree
    {
        #region Black-box

        class StaticIntervalTreeTester_BlackBox : IntervalCollectionTester
        {
            protected override Type GetCollectionType()
            {
                return typeof(StaticIntervalTree<,>);
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

        [TestFixture]
        class StaticIntervalTreeTester_WhiteBox
        {
            private TestHelper TH;

            #region SetUp

            [SetUp]
            public void SetUp()
            {
                TH = new TestHelper();
            }

            private static StaticIntervalTree<Interval, int> createSIT(Interval[] intervals = null)
            {
                if (intervals == null)
                    intervals = new Interval[0];

                return new StaticIntervalTree<Interval, int>(intervals);
            }

            private Interval[] A
            {
                get
                {
                    const int count = 7;
                    var intervals = new Interval[count];

                    for (var i = 0; i < count; i++)
                        intervals[i] = new Interval(i * 2 + 1, i * 2 + 2, IntervalType.Open);

                    return intervals;
                }
            }

            public Interval[] B
            {
                get
                {
                    return new[]{
                        new Interval(0, 10),
                        new Interval(0, 5, true, false),
                        new Interval(1, 5, true, true),
                        new Interval(2, 5, true, false),
                        new Interval(3, 5, true, true),
                        new Interval(4, 5, true, false),
                        new Interval(5, 5, true, true),
                        new Interval(5, 6, false, true),
                        new Interval(5, 7, true, true),
                        new Interval(5, 8, false, true),
                        new Interval(5, 9, true, true),
                        new Interval(5, 10, false, true),
                    };
                }
            }

            #endregion

            #region Count Overlaps

            #region Stabbing

            [Test]
            public void CountOverlapsStabbing_WhileRootNotNull_Zero()
            {
                var collection = createSIT();
                var query = TH.RandomInt;

                Assert.That(collection.CountOverlaps(query), Is.EqualTo(0));
            }

            [Test]
            public void CountOverlapsStabbing_CompareLessThanZero_Zero()
            {
                var collection = createSIT(A);

                Assert.That(collection.CountOverlaps(0), Is.EqualTo(0));
            }

            [Test]
            public void CountOverlapsStabbing_CompareGreaterThanZero_Zero()
            {
                var collection = createSIT(A);

                Assert.That(collection.CountOverlaps(15), Is.EqualTo(0));
            }

            [Test]
            public void CountOverlapsStabbing_CompareEqualsZero_Zero()
            {
                var collection = createSIT(A);

                Assert.That(collection.CountOverlaps(7), Is.EqualTo(0));
            }

            // TODO: Finish tests
            [Test]
            public void CountOverlapsStabbing_()
            {
                var collection = createSIT(B);

                Assert.That(collection.CountOverlaps(5), Is.EqualTo(6));
            }

            #endregion

            #region Range
            // TODO: Finish tests
            #endregion

            #endregion
        }

        #region Legacy Tests



        /*
        // Removed as method is private.
        [TestFixture]
        public class Median
        {
            [Test]
            public void Empty()
            {
            }

            private void checkK(C5.IList<int> list)
            {
                var sorted = new int[list.Count];
                list.CopyTo(sorted, 0);
                Array.Sort(sorted);

                for (int i = 0; i < list.Count; i++)
                {
                    var changedList = new ArrayList<int>();
                    list.ToList().ForEach(n => changedList.Add(n));
                    Assert.That(StaticIntervalTree<IInterval<int>, int>.GetK(changedList, i) == sorted[i]);
                }
            }

            [Test]
            public void AllSame()
            {
                checkK(new ArrayList<int> { 7, 7, 7, 7, 7 });
            }

            [Test]
            public void AllSameButOne()
            {
                checkK(new ArrayList<int> { 3, 7, 7, 7, 7 });
                checkK(new ArrayList<int> { 7, 3, 7, 7, 7 });
                checkK(new ArrayList<int> { 7, 7, 3, 7, 7 });
                checkK(new ArrayList<int> { 7, 7, 7, 3, 7 });
                checkK(new ArrayList<int> { 7, 7, 7, 7, 3 });
            }

            [Test]
            public void TwoDifferent()
            {
                checkK(new ArrayList<int> { 3, 3, 3, 7, 7 });
                checkK(new ArrayList<int> { 7, 7, 7, 3, 3 });
                checkK(new ArrayList<int> { 3, 7, 3, 7, 3 });
            }

            [Test]
            public void AllDifferent()
            {
                checkK(new ArrayList<int> { 11, 5, 9, 3, 7 });
                checkK(new ArrayList<int> { 3, 5, 7, 9, 11 });
            }

            [Test]
            public void Random()
            {
                Random randNum = new Random();
                var randoms = new C5.ArrayList<int>();
                Enumerable
                    .Repeat(0, 100)
                    .Select(i => randoms.Add(randNum.Next(0, 1000)));

                checkK(randoms);
            }
        }*/

        #endregion

        #endregion
    }
}
