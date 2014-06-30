using System;
using System.Linq;
using NUnit.Framework;

namespace C5.Intervals.Tests
{
    namespace DynamicIntervalTree
    {
        #region Black-box

        abstract class DynamicIntervalTreeTester_BlackBox : IntervalCollectionTester
        {
            protected override Type GetCollectionType()
            {
                return typeof(DynamicIntervalTree<,>);
            }

            // DIT's standard behavior where we set the ReferenceDuplicates to false
            protected override object[] AdditionalParameters()
            {
                return new object[] { AllowsReferenceDuplicates() };
            }

            protected override Speed CountSpeed()
            {
                return Speed.Constant;
            }
        }

        class DynamicIntervalTreeTester_BlackBox_ReferenceDuplicatesFalse : DynamicIntervalTreeTester_BlackBox
        {
            protected override bool AllowsReferenceDuplicates()
            {
                return false;
            }
        }

        class DynamicIntervalTreeTester_BlackBox_ReferenceDuplicatesTrue : DynamicIntervalTreeTester_BlackBox
        {
            protected override bool AllowsReferenceDuplicates()
            {
                return true;
            }
        }

        #endregion

        #region White-box

        // TODO: Hardcode examples from articles

        [TestFixture]
        class DynamicIntervalTreeTester
        {
            [SetUp]
            public void SetUp()
            {

            }

            [Test]
            public void Test() { }
        }

        [TestFixture]
        class ReportExample
        {
            private DynamicIntervalTree<IInterval<int>, int> _collection;

            [SetUp]
            public void SetUp()
            {
                _collection = new DynamicIntervalTree<IInterval<int>, int>();

                var points = new[] { 8, 4, 12, 2, 6, 10, 14, 1, 3, 9, 13, 15 };
                var pointIntervals = new IInterval<int>[points.Length];
                for (var i = 0; i < points.Length; i++)
                    pointIntervals[i] = new IntervalBase<int>(points[i]);

                foreach (var pointInterval in pointIntervals)
                    _collection.Add(pointInterval);

                var intervals = new[]
                    {
                        new IntervalBase<int>(1, 4, IntervalType.Closed),
                        new IntervalBase<int>(2),
                        new IntervalBase<int>(3, 4, IntervalType.Closed),
                        new IntervalBase<int>(3, 8, IntervalType.LowIncluded),
                        new IntervalBase<int>(4, 6, IntervalType.Closed),
                        new IntervalBase<int>(4, 13, IntervalType.Open),
                        new IntervalBase<int>(9, 10, IntervalType.Closed),
                        new IntervalBase<int>(10),
                        new IntervalBase<int>(12, 14, IntervalType.LowIncluded),
                        new IntervalBase<int>(12, 15, IntervalType.Open),
                        new IntervalBase<int>(13, 14, IntervalType.HighIncluded),
                        new IntervalBase<int>(14, 15, IntervalType.LowIncluded),
                    };

                foreach (var interval in intervals)
                    _collection.Add(interval);

                foreach (var pointInterval in pointIntervals)
                    _collection.Remove(pointInterval);
            }

            [Test]
            public void Test()
            {
                _collection.Add(new IntervalBase<int>(12));
                _collection.FindOverlaps(new IntervalBase<int>(9, 12, IntervalType.LowIncluded));

            }
        }

        [TestFixture]
        [Category("Former Bug")]
        class FormerBugs
        {
            [Test]
            public void Add_LowInsertedNodeButDidNotRotate_UnbalancedTree()
            {
                new DynamicIntervalTree<IInterval<int>, int>
                    {
                        new IntervalBase<int>(3, 5),
                        new IntervalBase<int>(1, 2),
                        new IntervalBase<int>(0, 4),
                        new IntervalBase<int>(-1, 5)
                    };
                Assert.Pass();
            }

            [Test]
            public void Remove_RotationDuringRemoveHighCausedUnupdatedSpan()
            {
                var interval1 = new IntervalBase<int>(6, 8);
                var interval2 = new IntervalBase<int>(3, 6);

                var collection = new DynamicIntervalTree<IInterval<int>, int>
                    {
                        interval1,
                        new IntervalBase<int>(4, 5),
                        new IntervalBase<int>(2, 5),
                        new IntervalBase<int>(7, 8),
                        new IntervalBase<int>(1, 4),
                        interval2
                    };

                collection.Remove(interval1);
                collection.Remove(interval2);
            }


            [Test]
            public void Add_SortedQueryingOutsideSpan()
            {
                var count = (int) Math.Pow(2.0, 5.0);
                var intervals = new IntervalBase<int>[count];

                for (var i = 0; i < count; i++)
                    intervals[i] = new IntervalBase<int>(i * 2, i * 2 + 1);

                var collection = new DynamicIntervalTree<IInterval<int>, int>();

                foreach (var interval in intervals)
                    collection.Add(interval);

                Assert.AreEqual(0, collection.FindOverlaps(-1).Count());
                Assert.AreEqual(0, collection.FindOverlaps(count * 2 + 2).Count());
            }
        }

        #endregion
    }
}
