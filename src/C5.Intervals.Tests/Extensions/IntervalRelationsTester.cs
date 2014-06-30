using System;
using C5.Intervals;
using NUnit.Framework;

namespace C5.Intervals.Tests
{
    using IntervalOfInt = IntervalBase<int>;

    [TestFixture]
    class IntervalRelationsTester
    {
        [Test]
        public void Symbols()
        {
            Console.WriteLine(IntervalRelation.After.Symbol());
        }

        [Test]
        public void DraftTest()
        {
            var x = new IntervalOfInt(3, 6, IntervalType.Closed);

            var intervalAfter = new IntervalOfInt(1, 2, IntervalType.LowIncluded);
            Assert.AreEqual(IntervalRelation.After, x.RelateTo(intervalAfter));

            var intervalMetBy = new IntervalOfInt(1, 3, IntervalType.LowIncluded);
            Assert.AreEqual(IntervalRelation.MetBy, x.RelateTo(intervalMetBy));

            var intervalOverlappedBy = new IntervalOfInt(1, 3, IntervalType.Closed);
            Assert.AreEqual(IntervalRelation.MetBy, x.RelateTo(intervalMetBy));

            intervalOverlappedBy = new IntervalOfInt(1, 4, IntervalType.Closed);
            Assert.AreEqual(IntervalRelation.OverlappedBy, x.RelateTo(intervalOverlappedBy));

            var intervalFinishes = new IntervalOfInt(1, 6, IntervalType.Closed);
            Assert.AreEqual(IntervalRelation.Finishes, x.RelateTo(intervalFinishes));

            var intervalDuring = new IntervalOfInt(1, 8, IntervalType.Closed);
            Assert.AreEqual(IntervalRelation.During, x.RelateTo(intervalDuring));

            var intervalStartedBy = new IntervalOfInt(3, 5, IntervalType.Closed);
            Assert.AreEqual(IntervalRelation.StartedBy, x.RelateTo(intervalStartedBy));

            var intervalEquals = new IntervalOfInt(3, 6, IntervalType.Closed);
            Assert.AreEqual(IntervalRelation.Equals, x.RelateTo(intervalEquals));

            var intervalStarts = new IntervalOfInt(3, 7, IntervalType.Closed);
            Assert.AreEqual(IntervalRelation.Starts, x.RelateTo(intervalStarts));

            var intervalContains = new IntervalOfInt(4, 5, IntervalType.Closed);
            Assert.AreEqual(IntervalRelation.Contains, x.RelateTo(intervalContains));

            var intervalFinishedBy = new IntervalOfInt(4, 6, IntervalType.Closed);
            Assert.AreEqual(IntervalRelation.FinishedBy, x.RelateTo(intervalFinishedBy));

            var intervalOverlaps = new IntervalOfInt(4, 7, IntervalType.Closed);
            Assert.AreEqual(IntervalRelation.Overlaps, x.RelateTo(intervalOverlaps));

            intervalOverlaps = new IntervalOfInt(6, 7, IntervalType.Closed);
            Assert.AreEqual(IntervalRelation.Overlaps, x.RelateTo(intervalOverlaps));

            var intervalMeets = new IntervalOfInt(6, 7, IntervalType.HighIncluded);
            Assert.AreEqual(IntervalRelation.Meets, x.RelateTo(intervalMeets));

            var intervalBefore = new IntervalOfInt(7, 9, IntervalType.Closed);
            Assert.AreEqual(IntervalRelation.Before, x.RelateTo(intervalBefore));

            // TODO: Add tests for Meet and MetBy with points

            var pointAfter = new IntervalOfInt(1);
            Assert.AreEqual(IntervalRelation.After, x.RelateTo(pointAfter));

            var pointStartedBy = new IntervalOfInt(3);
            Assert.AreEqual(IntervalRelation.StartedBy, x.RelateTo(pointStartedBy));

            var pointContains = new IntervalOfInt(5);
            Assert.AreEqual(IntervalRelation.Contains, x.RelateTo(pointContains));

            var pointFinishedBy = new IntervalOfInt(6);
            Assert.AreEqual(IntervalRelation.FinishedBy, x.RelateTo(pointFinishedBy));

            var pointBefore = new IntervalOfInt(8);
            Assert.AreEqual(IntervalRelation.Before, x.RelateTo(pointBefore));


            x = new IntervalOfInt(3);

            pointAfter = new IntervalOfInt(1);
            Assert.AreEqual(IntervalRelation.After, x.RelateTo(pointAfter));

            var pointEquals = new IntervalOfInt(3);
            Assert.AreEqual(IntervalRelation.Equals, x.RelateTo(pointEquals));

            pointBefore = new IntervalOfInt(5);
            Assert.AreEqual(IntervalRelation.Before, x.RelateTo(pointBefore));


            intervalAfter = new IntervalOfInt(1, 2, IntervalType.Closed);
            Assert.AreEqual(IntervalRelation.After, x.RelateTo(intervalAfter));

            intervalFinishes = new IntervalOfInt(1, 3, IntervalType.Closed);
            Assert.AreEqual(IntervalRelation.Finishes, x.RelateTo(intervalFinishes));

            intervalDuring = new IntervalOfInt(1, 4, IntervalType.Closed);
            Assert.AreEqual(IntervalRelation.During, x.RelateTo(intervalDuring));

            intervalStarts = new IntervalOfInt(3, 4, IntervalType.Closed);
            Assert.AreEqual(IntervalRelation.Starts, x.RelateTo(intervalStarts));

            intervalBefore = new IntervalOfInt(4, 6, IntervalType.Closed);
            Assert.AreEqual(IntervalRelation.Before, x.RelateTo(intervalBefore));
        }

        [Test]
        public void After()
        {

            var x = new IntervalOfInt(3, 5, IntervalType.Closed);
            var y = new IntervalOfInt(3);

            Console.WriteLine(x.RelateTo(y));
            Console.WriteLine(x.CompareTo(y));
        }
    }
}
