using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace C5.Intervals.Tests.Extensions
{
    [TestFixture]
    class IntervalTypeExtensionTester
    {
        [Test]
        public void Test()
        {
            var interval = new IntervalBase<int>(1, 2, IntervalType.Closed);

            Assert.That(interval.IntervalType(), Is.EqualTo(IntervalType.Closed));
        }

        [Test]
        public void CollectionTests()
        {
            var collection1 = new[]
            {
                new IntervalBase<int>(1),
                new IntervalBase<int>(2),
                new IntervalBase<int>(3),
            };
            var collection2 = new[]
            {
                new IntervalBase<int>(1),
                new IntervalBase<int>(2),
                new IntervalBase<int>(3),
            };

            Assert.That(collection1, Is.EqualTo(collection2));

            CollectionAssert.AreEqual(collection1, collection2);
        }
    }
}
