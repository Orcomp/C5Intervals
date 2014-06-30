using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace C5.Intervals.Tests
{
    using Interval = IntervalBase<int>;

    [TestFixture]
    public class IntervalEquals
    {
        [Test]
        public void Intervals()
        {
            IInterval<int> interval1 = new IntervalBase<int>(1, 2, true, true);
            IInterval<int> interval2 = new IntervalBase<int>(1, 2, true, true);
            IInterval<int> interval3 = new IntervalBase<int>(1, 2, true, false);
            IInterval<int> interval4 = new IntervalBase<int>(1, 3, true, true);

            Assert.AreEqual(interval1, interval2);
            Assert.AreNotEqual(interval1, interval3);
            Assert.AreNotEqual(interval1, interval4);
        }
    }

    [TestFixture]
    public class Gaps
    {
        readonly Random _random = new Random();

        [Test]
        public void Test()
        {
            const int count = 101;
            var intervals = this.intervals(count);
            var span = intervals.Span();

            var input = new ArrayList<IInterval<int>>();
            var expected = new ArrayList<IInterval<int>>();

            for (var i = 0; i < count; i++)
                if (i % 2 == 0)
                    expected.Add(intervals[i]);
                else
                    input.Add(intervals[i]);

            Assert.True(span.StrictlyContains(input.Span()));

            CollectionAssert.AreEquivalent(expected, input.Gaps(span));
            CollectionAssert.AreEquivalent(input, expected.Gaps());
        }

        [Test]
        public void Gaps_NoGaps([Values(true, false)] bool isSorted)
        {
            CollectionAssert.IsEmpty(intervals().Gaps(isSorted: isSorted));
        }

        [Test, Combinatorial]
        public void GapsSpan_MatchCounterpart(
            [Values(0, 1)] int count,
            [Values(true, false)] bool add,
            [Values(true, false)] bool isSorted
            )
        {
            var intervals = this.intervals(100 + count);
            var span = intervals.Span();

            var expected = new ArrayList<IInterval<int>>();
            var input = new ArrayList<IInterval<int>>();

            foreach (var interval in intervals)
                ((add = !add) ? expected : input).Add(interval);

            CollectionAssert.AreEquivalent(expected, input.Gaps(span, isSorted));
        }

        [Test, Combinatorial]
        public void Gaps_MatchCounterpart([Values(true, false)] bool isSorted)
        {
            var intervals = this.intervals();

            var expected = new ArrayList<IInterval<int>>();
            var input = new ArrayList<IInterval<int>>();
            var add = true;
            foreach (var interval in intervals)
                ((add = !add) ? expected : input).Add(interval);

            CollectionAssert.AreEquivalent(expected, input.Gaps(isSorted: isSorted));
        }

        private IInterval<int>[] intervals(int count = 101, int length = 10)
        {
            var intervals = new IInterval<int>[count];

            intervals[0] = new IntervalBase<int>(0, _random.Next(1, 1 + length));
            for (var i = 1; i < count; i++)
            {
                var lastHigh = intervals[i - 1].High;
                intervals[i] = new IntervalBase<int>(lastHigh, _random.Next(lastHigh + 1, lastHigh + 1 + length));
            }

            return intervals;
        }
    }

    [TestFixture]
    public class Collapse
    {
        [Test]
        public void Collapse_Ben_AllInOne()
        {
            var intervals = new[]
            {
                new Interval(0, 1, IntervalType.Closed),
                new Interval(1, 2, IntervalType.HighIncluded),

                new Interval(3, 8, IntervalType.Closed),
                new Interval(4, 7, IntervalType.Closed), 
                new Interval(5, 6, IntervalType.Closed),

                new Interval(9, 11),
                new Interval(10, 12),
            };

            var collapsed = intervals.Collapse<Interval, int>(false);

            var expected = new[]
            {
                new KeyValuePair<IInterval<int>, IEnumerable<Interval>>(intervals[0], new[] {intervals[0]}),
                new KeyValuePair<IInterval<int>, IEnumerable<Interval>>(intervals[1], new[] {intervals[1]}),
                
                new KeyValuePair<IInterval<int>, IEnumerable<Interval>>(new Interval(3, 4, IntervalType.LowIncluded), new[] {intervals[2]}),
                new KeyValuePair<IInterval<int>, IEnumerable<Interval>>(new Interval(4, 5, IntervalType.LowIncluded), new[] {intervals[2], intervals[3]}),
                new KeyValuePair<IInterval<int>, IEnumerable<Interval>>(new Interval(5, 6, IntervalType.Closed), new[] {intervals[2], intervals[3], intervals[4]}),
                new KeyValuePair<IInterval<int>, IEnumerable<Interval>>(new Interval(6, 7, IntervalType.HighIncluded), new[] {intervals[2], intervals[3]}),
                new KeyValuePair<IInterval<int>, IEnumerable<Interval>>(new Interval(7, 8, IntervalType.HighIncluded), new[] {intervals[2]}),
                
                new KeyValuePair<IInterval<int>, IEnumerable<Interval>>(new Interval(9, 10, IntervalType.LowIncluded), new[] {intervals[5]}),
                new KeyValuePair<IInterval<int>, IEnumerable<Interval>>(new Interval(10, 11, IntervalType.LowIncluded), new[] {intervals[5], intervals[6]}),
                new KeyValuePair<IInterval<int>, IEnumerable<Interval>>(new Interval(11, 12, IntervalType.LowIncluded), new[] {intervals[6]}),
            };

            AssertEqual(expected, collapsed);
        }

        [Test]
        public void Collapse_Ben_Containment()
        {
            var intervals = new[]
            {
                new Interval(0, 5, IntervalType.Closed),
                new Interval(1, 4, IntervalType.Closed), 
                new Interval(2, 3, IntervalType.Closed),
            };

            var collapsed = intervals.Collapse<Interval, int>(false);

            var expected = new[]
            {
                new KeyValuePair<IInterval<int>, IEnumerable<Interval>>(new Interval(0, 1, IntervalType.LowIncluded), new[] {intervals[0]}),
                new KeyValuePair<IInterval<int>, IEnumerable<Interval>>(new Interval(1, 2, IntervalType.LowIncluded), new[] {intervals[0], intervals[1]}),
                new KeyValuePair<IInterval<int>, IEnumerable<Interval>>(new Interval(2, 3, IntervalType.Closed), intervals),
                new KeyValuePair<IInterval<int>, IEnumerable<Interval>>(new Interval(3, 4, IntervalType.HighIncluded), new[] {intervals[0], intervals[1]}),
                new KeyValuePair<IInterval<int>, IEnumerable<Interval>>(new Interval(4, 5, IntervalType.HighIncluded), new[] {intervals[0]}),
            };

            AssertEqual(expected, collapsed);
        }

        [Test]
        public void Collapse_Ben_Overlaps()
        {
            var intervals = new[]
            {
                new Interval(0, 2),
                new Interval(1, 3),
            };

            var collapsed = intervals.Collapse<Interval, int>();

            var expected = new[]
            {
                new KeyValuePair<IInterval<int>, IEnumerable<Interval>>(new Interval(0, 1, IntervalType.LowIncluded), new[] {intervals[0]}),
                new KeyValuePair<IInterval<int>, IEnumerable<Interval>>(new Interval(1, 2, IntervalType.LowIncluded), intervals),
                new KeyValuePair<IInterval<int>, IEnumerable<Interval>>(new Interval(2, 3, IntervalType.LowIncluded), new[] {intervals[1]}),
            };

            AssertEqual(expected, collapsed);
        }

        [Test]
        public void Collapse_Ben_NoOverlaps()
        {
            var intervals = new[]
            {
                new Interval(0, 1),
                new Interval(2, 3),
                new Interval(3, 4),
            };

            var collapsed = intervals.Collapse<Interval, int>();

            var expected = new[]
            {
                new KeyValuePair<IInterval<int>, IEnumerable<Interval>>(intervals[0], new[]{intervals[0]}),
                new KeyValuePair<IInterval<int>, IEnumerable<Interval>>(intervals[1], new[]{intervals[1]}),
                new KeyValuePair<IInterval<int>, IEnumerable<Interval>>(intervals[2], new[]{intervals[2]}),
            };

            AssertEqual(expected, collapsed);
        }

        [Test]
        public void Collapse_Equals()
        {
            var intervals = new[]
            {
                new Interval(0, 1),
                new Interval(0, 1),
            };

            var collapsed = intervals.Collapse<Interval, int>();

            var expected = new[]
            {
                new KeyValuePair<IInterval<int>, IEnumerable<Interval>>(intervals[0], intervals),
            };

            AssertEqual(expected, collapsed);
        }

        private static void AssertEqual(IEnumerable<KeyValuePair<IInterval<int>, IEnumerable<Interval>>> expected,
            IEnumerable<KeyValuePair<IInterval<int>, IEnumerable<Interval>>> actual)
        {
            var expectedArray = expected.ToArray();
            var actualArray = actual.ToArray();

            Assert.That(actualArray.Length, Is.EqualTo(expectedArray.Length));

            for (var i = 0; i < expectedArray.Length; i++)
            {
                var a = expectedArray[i];
                var b = actualArray[i];

                Assert.That(a.Key.IntervalEquals(b.Key));

                Assert.That(b.Value, Is.EquivalentTo(a.Value));
            }
        }
    }

    [TestFixture]
    public class Intersection
    {
        [Test]
        public void Test()
        {
            Console.WriteLine((new IntervalBase<int>(2, 7)).Overlap(new IntervalBase<int>(1, 3)));
        }
    }

    // ReSharper disable CoVariantArrayConversion
    /// <summary>
    /// I before J
    /// IIIIII    JJJJJJ
    /// </summary>
    [TestFixture]
    public class IntervalIBeforeJ
    {
        private static readonly IInterval<int>[] I = new[] {
                new IntervalBase<int>(0, 5, false, false), // ()
                new IntervalBase<int>(0, 5, false, true), // (]
                new IntervalBase<int>(0, 5, true, false), // [)
                new IntervalBase<int>(0, 5, true, true) // []
            };
        private static readonly IInterval<int>[] J = new[] {
                new IntervalBase<int>(10, 15, false, false), // ()
                new IntervalBase<int>(10, 15, false, true), // (]
                new IntervalBase<int>(10, 15, true, false), // [)
                new IntervalBase<int>(10, 15, true, true) // []
            };

        [Test, Combinatorial, Category("IntervalComparer<T>.Overlaps")]
        public void Overlaps_Before_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            Assert.IsFalse(swap ? i.Overlaps(j) : j.Overlaps(i));
        }

        [Test, Combinatorial, Category("IntervalComparer<T>.StaticCompare")]
        public void StaticCompare_Before_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            if (swap)
                Assert.Less(i.CompareTo(j), 0);
            else
                Assert.Greater(j.CompareTo(i), 0);
        }

        [Test, Combinatorial, Category("IntervalComparer<T>.StrictlyContains")]
        public void Contains_Before_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            Assert.IsFalse(swap ? i.StrictlyContains(j) : j.StrictlyContains(i));
        }
    }

    /// <summary>
    /// I equal J
    /// IIIIII
    /// JJJJJJ
    /// </summary>
    [TestFixture]
    public class IntervalXEqualY
    {
        private static readonly IInterval<int>[] I = new[] {
                new IntervalBase<int>(0, 5, false, false), // ()
                new IntervalBase<int>(0, 5, false, true), // (]
                new IntervalBase<int>(0, 5, true, false), // [)
                new IntervalBase<int>(0, 5, true, true) // []
            };
        private static readonly IInterval<int>[] J = new[] {
                new IntervalBase<int>(0, 5, false, false), // ()
                new IntervalBase<int>(0, 5, false, true), // (]
                new IntervalBase<int>(0, 5, true, false), // [)
                new IntervalBase<int>(0, 5, true, true) // []
            };

        [Test, Combinatorial, Category("IntervalComparer<T>.Overlaps")]
        public void Overlap_Equal_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            Assert.That(swap ? i.Overlaps(j) : j.Overlaps(i));
        }

        [TestCaseSource("StabCases"), Category("IntervalComparer<T>.StaticCompare")]
        public void StaticCompare_Equal_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expression)
        {
            Assert.That(i.CompareTo(j), expression);
        }

        public static readonly object[] StabCases = new object[] {
                new object[] { I[0], J[0], Is.EqualTo(0)},
                new object[] { I[0], J[1], Is.LessThan(0)},
                new object[] { I[0], J[2], Is.GreaterThan(0)},
                new object[] { I[0], J[3], Is.GreaterThan(0)},
                new object[] { I[1], J[0], Is.GreaterThan(0)},
                new object[] { I[1], J[1], Is.EqualTo(0)},
                new object[] { I[1], J[2], Is.GreaterThan(0)},
                new object[] { I[1], J[3], Is.GreaterThan(0)},
                new object[] { I[2], J[0], Is.LessThan(0)},
                new object[] { I[2], J[1], Is.LessThan(0)},
                new object[] { I[2], J[2], Is.EqualTo(0)},
                new object[] { I[2], J[3], Is.LessThan(0)},
                new object[] { I[3], J[0], Is.LessThan(0)},
                new object[] { I[3], J[1], Is.LessThan(0)},
                new object[] { I[3], J[2], Is.GreaterThan(0)},
                new object[] { I[3], J[3], Is.EqualTo(0)},

                new object[] { J[0], I[0], Is.EqualTo(0)},
                new object[] { J[0], I[1], Is.LessThan(0)},
                new object[] { J[0], I[2], Is.GreaterThan(0)},
                new object[] { J[0], I[3], Is.GreaterThan(0)},
                new object[] { J[1], I[0], Is.GreaterThan(0)},
                new object[] { J[1], I[1], Is.EqualTo(0)},
                new object[] { J[1], I[2], Is.GreaterThan(0)},
                new object[] { J[1], I[3], Is.GreaterThan(0)},
                new object[] { J[2], I[0], Is.LessThan(0)},
                new object[] { J[2], I[1], Is.LessThan(0)},
                new object[] { J[2], I[2], Is.EqualTo(0)},
                new object[] { J[2], I[3], Is.LessThan(0)},
                new object[] { J[3], I[0], Is.LessThan(0)},
                new object[] { J[3], I[1], Is.LessThan(0)},
                new object[] { J[3], I[2], Is.GreaterThan(0)},
                new object[] { J[3], I[3], Is.EqualTo(0)}
            };

        //StrictlyContains
        [TestCaseSource("ContainsCases"), Category("IntervalComparer<T>.StrictlyContains")]
        public void Contains_Equal_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.StrictlyContains(j), expected);
        }

        public static object[] ContainsCases = new object[] {
                new object[] { I[0], J[0], Is.False},
                new object[] { I[0], J[1], Is.False},
                new object[] { I[0], J[2], Is.False},
                new object[] { I[0], J[3], Is.False},
                new object[] { I[1], J[0], Is.False},
                new object[] { I[1], J[1], Is.False},
                new object[] { I[1], J[2], Is.False},
                new object[] { I[1], J[3], Is.False},
                new object[] { I[2], J[0], Is.False},
                new object[] { I[2], J[1], Is.False},
                new object[] { I[2], J[2], Is.False},
                new object[] { I[2], J[3], Is.False},
                new object[] { I[3], J[0], Is.True},
                new object[] { I[3], J[1], Is.False},
                new object[] { I[3], J[2], Is.False},
                new object[] { I[3], J[3], Is.False},

                new object[] { J[0], I[0], Is.False},
                new object[] { J[0], I[1], Is.False},
                new object[] { J[0], I[2], Is.False},
                new object[] { J[0], I[3], Is.False},
                new object[] { J[1], I[0], Is.False},
                new object[] { J[1], I[1], Is.False},
                new object[] { J[1], I[2], Is.False},
                new object[] { J[1], I[3], Is.False},
                new object[] { J[2], I[0], Is.False},
                new object[] { J[2], I[1], Is.False},
                new object[] { J[2], I[2], Is.False},
                new object[] { J[2], I[3], Is.False},
                new object[] { J[3], I[0], Is.True},
                new object[] { J[3], I[1], Is.False},
                new object[] { J[3], I[2], Is.False},
                new object[] { J[3], I[3], Is.False}
            };
    }

    /// <summary>
    /// I meets J
    /// IIIIII
    ///      JJJJJJ
    /// </summary>
    [TestFixture]
    public class IntervalXMeetsY
    {
        private static readonly IInterval<int>[] I = new[] {
                new IntervalBase<int>(0, 5, false, false), // ()
                new IntervalBase<int>(0, 5, false, true), // (]
                new IntervalBase<int>(0, 5, true, false), // [)
                new IntervalBase<int>(0, 5, true, true) // []
            };
        private static readonly IInterval<int>[] J = new[] {
                new IntervalBase<int>(5, 10, false, false), // ()
                new IntervalBase<int>(5, 10, false, true), // (]
                new IntervalBase<int>(5, 10, true, false), // [)
                new IntervalBase<int>(5, 10, true, true) // []
            };

        [TestCaseSource("OverlapCases"), Category("IntervalComparer<T>.Overlaps")]
        public void Overlaps_Meets_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.Overlaps(j), expected);
        }

        public static object[] OverlapCases = new object[] {
                new object[] { I[0], J[0], Is.False},
                new object[] { I[0], J[1], Is.False},
                new object[] { I[0], J[2], Is.False},
                new object[] { I[0], J[3], Is.False},
                new object[] { I[1], J[0], Is.False},
                new object[] { I[1], J[1], Is.False},
                new object[] { I[1], J[2], Is.True},
                new object[] { I[1], J[3], Is.True},
                new object[] { I[2], J[0], Is.False},
                new object[] { I[2], J[1], Is.False},
                new object[] { I[2], J[2], Is.False},
                new object[] { I[2], J[3], Is.False},
                new object[] { I[3], J[0], Is.False},
                new object[] { I[3], J[1], Is.False},
                new object[] { I[3], J[2], Is.True},
                new object[] { I[3], J[3], Is.True},

                new object[] { J[0], I[0], Is.False},
                new object[] { J[0], I[1], Is.False},
                new object[] { J[0], I[2], Is.False},
                new object[] { J[0], I[3], Is.False},
                new object[] { J[1], I[0], Is.False},
                new object[] { J[1], I[1], Is.False},
                new object[] { J[1], I[2], Is.False},
                new object[] { J[1], I[3], Is.False},
                new object[] { J[2], I[0], Is.False},
                new object[] { J[2], I[1], Is.True},
                new object[] { J[2], I[2], Is.False},
                new object[] { J[2], I[3], Is.True},
                new object[] { J[3], I[0], Is.False},
                new object[] { J[3], I[1], Is.True},
                new object[] { J[3], I[2], Is.False},
                new object[] { J[3], I[3], Is.True}
            };


        [Test, Combinatorial, Category("IntervalComparer<T>.StaticCompare")]
        public void StaticCompare_Meets_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            if (swap)
                Assert.Less(i.CompareTo(j), 0);
            else
                Assert.Greater(j.CompareTo(i), 0);
        }


        [Test, Combinatorial, Category("IntervalComparer<T>.StrictlyContains")]
        public void Contains_Meets_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            Assert.IsFalse(swap ? i.StrictlyContains(j) : j.StrictlyContains(i));
        }
    }

    /// <summary>
    /// I overlaps J
    /// IIIIIIIIIII
    ///      JJJJJJJJJJJ
    /// </summary>
    [TestFixture]
    public class IntervalXOverlapsY
    {
        private static readonly IInterval<int>[] I = new[] {
                new IntervalBase<int>(0, 10, false, false), // ()
                new IntervalBase<int>(0, 10, false, true), // (]
                new IntervalBase<int>(0, 10, true, false), // [)
                new IntervalBase<int>(0, 10, true, true) // []
            };
        private static readonly IInterval<int>[] J = new[] {
                new IntervalBase<int>(5, 15, false, false), // ()
                new IntervalBase<int>(5, 15, false, true), // (]
                new IntervalBase<int>(5, 15, true, false), // [)
                new IntervalBase<int>(5, 15, true, true) // []
            };

        [Test, Combinatorial, Category("IntervalComparer<T>.Overlaps")]
        public void Overlaps_Overlap_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            Assert.That(swap ? i.Overlaps(j) : j.Overlaps(i));
        }

        [Test, Combinatorial, Category("IntervalComparer<T>.StaticCompare")]
        public void StaticCompare_Overlap_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            if (swap)
                Assert.Less(i.CompareTo(j), 0);
            else
                Assert.Greater(j.CompareTo(i), 0);
        }

        [Test, Combinatorial, Category("IntervalComparer<T>.StrictlyContains")]
        public void Contains_Overlap_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            Assert.IsFalse(swap ? i.StrictlyContains(j) : j.StrictlyContains(i));
        }
    }

    /// <summary>
    /// I during J
    ///      IIIIII
    /// JJJJJJJJJJJJJJJJ
    /// </summary>
    [TestFixture]
    public class IntervalXDuringY
    {
        private static readonly IInterval<int>[] I = new[] {
                new IntervalBase<int>(5, 10, false, false), // ()
                new IntervalBase<int>(5, 10, false, true), // (]
                new IntervalBase<int>(5, 10, true, false), // [)
                new IntervalBase<int>(5, 10, true, true) // []
            };
        private static readonly IInterval<int>[] J = new[] {
                new IntervalBase<int>(0, 15, false, false), // ()
                new IntervalBase<int>(0, 15, false, true), // (]
                new IntervalBase<int>(0, 15, true, false), // [)
                new IntervalBase<int>(0, 15, true, true) // []
            };

        [Test, Combinatorial, Category("IntervalComparer<T>.Overlaps")]
        public void Overlaps_During_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            Assert.That(swap ? i.Overlaps(j) : j.Overlaps(i));
        }

        [Test, Combinatorial, Category("IntervalComparer<T>.StaticCompare")]
        public void StaticCompare_During_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            if (swap)
                Assert.Greater(i.CompareTo(j), 0);
            else
                Assert.Less(j.CompareTo(i), 0);
        }

        [Test, Combinatorial, Category("IntervalComparer<T>.StrictlyContains")]
        public void Contains_During_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            if (swap)
                Assert.IsFalse(i.StrictlyContains(j));
            else
                Assert.That(j.StrictlyContains(i));
        }
    }

    /// <summary>
    /// I starts J
    /// IIIIII
    /// JJJJJJJJJJJ
    /// </summary>
    [TestFixture]
    public class IntervalXStartsY
    {
        private static readonly IInterval<int>[] I = new[] {
                new IntervalBase<int>(0, 5, false, false), // ()
                new IntervalBase<int>(0, 5, false, true), // (]
                new IntervalBase<int>(0, 5, true, false), // [)
                new IntervalBase<int>(0, 5, true, true) // []
            };
        private static readonly IInterval<int>[] J = new[] {
                new IntervalBase<int>(0, 10,false, false), // ()
                new IntervalBase<int>(0, 10,false, true), // (]
                new IntervalBase<int>(0, 10,true, false), // [)
                new IntervalBase<int>(0, 10,true, true) // []
            };

        [Test, Combinatorial, Category("IntervalComparer<T>.Overlaps")]
        public void Overlaps_Starts_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            Assert.That(swap ? i.Overlaps(j) : j.Overlaps(i));
        }

        [TestCaseSource("StabCases")]
        public void StaticComparer_Starts_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.CompareTo(j), expected);
        }

        public static object[] StabCases = new object[] {
                new object[] { I[0], J[0], Is.LessThan(0)},
                new object[] { I[0], J[1], Is.LessThan(0)},
                new object[] { I[0], J[2], Is.GreaterThan(0)},
                new object[] { I[0], J[3], Is.GreaterThan(0)},
                new object[] { I[1], J[0], Is.LessThan(0)},
                new object[] { I[1], J[1], Is.LessThan(0)},
                new object[] { I[1], J[2], Is.GreaterThan(0)},
                new object[] { I[1], J[3], Is.GreaterThan(0)},
                new object[] { I[2], J[0], Is.LessThan(0)},
                new object[] { I[2], J[1], Is.LessThan(0)},
                new object[] { I[2], J[2], Is.LessThan(0)},
                new object[] { I[2], J[3], Is.LessThan(0)},
                new object[] { I[3], J[0], Is.LessThan(0)},
                new object[] { I[3], J[1], Is.LessThan(0)},
                new object[] { I[3], J[2], Is.LessThan(0)},
                new object[] { I[3], J[3], Is.LessThan(0)},

                new object[] { J[0], I[0], Is.GreaterThan(0)},
                new object[] { J[0], I[1], Is.GreaterThan(0)},
                new object[] { J[0], I[2], Is.GreaterThan(0)},
                new object[] { J[0], I[3], Is.GreaterThan(0)},
                new object[] { J[1], I[0], Is.GreaterThan(0)},
                new object[] { J[1], I[1], Is.GreaterThan(0)},
                new object[] { J[1], I[2], Is.GreaterThan(0)},
                new object[] { J[1], I[3], Is.GreaterThan(0)},
                new object[] { J[2], I[0], Is.LessThan(0)},
                new object[] { J[2], I[1], Is.LessThan(0)},
                new object[] { J[2], I[2], Is.GreaterThan(0)},
                new object[] { J[2], I[3], Is.GreaterThan(0)},
                new object[] { J[3], I[0], Is.LessThan(0)},
                new object[] { J[3], I[1], Is.LessThan(0)},
                new object[] { J[3], I[2], Is.GreaterThan(0)},
                new object[] { J[3], I[3], Is.GreaterThan(0)}
            };


        [Test, Combinatorial, Category("IntervalComparer<T>.StrictlyContains")]
        public void Contains_Starts_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j)
        {
            Assert.IsFalse(i.StrictlyContains(j));
        }

        [TestCaseSource("ContainsCases"), Category("IntervalComparer<T>.StrictlyContains")]
        public void Contains_Starts_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.StrictlyContains(j), expected);
        }

        public static object[] ContainsCases = new object[] {
                new object[] { J[0], I[0], Is.False},
                new object[] { J[0], I[1], Is.False},
                new object[] { J[0], I[2], Is.False},
                new object[] { J[0], I[3], Is.False},
                new object[] { J[1], I[0], Is.False},
                new object[] { J[1], I[1], Is.False},
                new object[] { J[1], I[2], Is.False},
                new object[] { J[1], I[3], Is.False},
                new object[] { J[2], I[0], Is.True},
                new object[] { J[2], I[1], Is.True},
                new object[] { J[2], I[2], Is.False},
                new object[] { J[2], I[3], Is.False},
                new object[] { J[3], I[0], Is.True},
                new object[] { J[3], I[1], Is.True},
                new object[] { J[3], I[2], Is.False},
                new object[] { J[3], I[3], Is.False}
            };
    }

    /// <summary>
    /// I finishes J
    ///      IIIIII
    /// JJJJJJJJJJJ
    /// </summary>
    [TestFixture]
    public class IntervalXFinishesY
    {
        private static readonly IInterval<int>[] I = new[] {
                new IntervalBase<int>(5, 10, false, false), // ()
                new IntervalBase<int>(5, 10, false, true), // (]
                new IntervalBase<int>(5, 10, true, false), // [)
                new IntervalBase<int>(5, 10, true, true) // []
            };
        private static readonly IInterval<int>[] J = new[] {
                new IntervalBase<int>(0, 10, false, false), // ()
                new IntervalBase<int>(0, 10, false, true), // (]
                new IntervalBase<int>(0, 10, true, false), // [)
                new IntervalBase<int>(0, 10, true, true) // []
            };

        [Test, Combinatorial, Category("IntervalComparer<T>.Overlaps")]
        public void Overlaps_Finishes_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            Assert.That(swap ? i.Overlaps(j) : j.Overlaps(i));
        }

        [Test, Combinatorial, Category("IntervalComparer<T>.StaticCompare")]
        public void StaticCompare_Finishes_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            if (swap)
                Assert.Greater(i.CompareTo(j), 0);
            else
                Assert.Less(j.CompareTo(i), 0);
        }

        [Test, Combinatorial, Category("IntervalComparer<T>.StrictlyContains")]
        public void Contains_Finishes_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j)
        {
            Assert.IsFalse(i.StrictlyContains(j));
        }

        // StrictlyContains
        [TestCaseSource("ContainsCases"), Category("IntervalComparer<T>.StrictlyContains")]
        public void Contains_Finishes_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.StrictlyContains(j), expected);
        }

        public static object[] ContainsCases = new object[] {
                new object[] { J[0], I[0], Is.False},
                new object[] { J[0], I[1], Is.False},
                new object[] { J[0], I[2], Is.False},
                new object[] { J[0], I[3], Is.False},
                new object[] { J[1], I[0], Is.True},
                new object[] { J[1], I[1], Is.False},
                new object[] { J[1], I[2], Is.True},
                new object[] { J[1], I[3], Is.False},
                new object[] { J[2], I[0], Is.False},
                new object[] { J[2], I[1], Is.False},
                new object[] { J[2], I[2], Is.False},
                new object[] { J[2], I[3], Is.False},
                new object[] { J[3], I[0], Is.True},
                new object[] { J[3], I[1], Is.False},
                new object[] { J[3], I[2], Is.True},
                new object[] { J[3], I[3], Is.False}
            };
    }

    /// <summary>
    /// P before I
    /// P
    ///      IIIIII
    /// </summary>
    [TestFixture]
    public class IntervalPBeforeX
    {
        private static readonly IInterval<int> P = new IntervalBase<int>(0);
        private static readonly IInterval<int>[] I = new[] {
                new IntervalBase<int>(5, 10, false, false), // ()
                new IntervalBase<int>(5, 10, false, true), // (]
                new IntervalBase<int>(5, 10, true, false), // [)
                new IntervalBase<int>(5, 10, true, true) // []
            };

        //FindOverlaps
        [TestCaseSource(typeof(IntervalPBeforeX), "OverlapCases")]
        public void Overlap_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.Overlaps(j), expected);
        }

        public static object[] OverlapCases = new object[] {
                new object[] { I[0], P, Is.False},
                new object[] { I[1], P, Is.False},
                new object[] { I[2], P, Is.False},
                new object[] { I[3], P, Is.False},
                new object[] { P, I[0], Is.False},
                new object[] { P, I[1], Is.False},
                new object[] { P, I[2], Is.False},
                new object[] { P, I[3], Is.False}
            };

        //Sorting
        [TestCaseSource(typeof(IntervalPBeforeX), "SortingCases")]
        public void Sorting_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.CompareTo(j), expected);
        }

        public static object[] SortingCases = new object[] {
                new object[] { I[0], P, Is.GreaterThan(0)},
                new object[] { I[1], P, Is.GreaterThan(0)},
                new object[] { I[2], P, Is.GreaterThan(0)},
                new object[] { I[3], P, Is.GreaterThan(0)},
                new object[] { P, I[0], Is.LessThan(0)},
                new object[] { P, I[1], Is.LessThan(0)},
                new object[] { P, I[2], Is.LessThan(0)},
                new object[] { P, I[3], Is.LessThan(0)}
            };

        //StrictlyContains
        [TestCaseSource("ContainsCases"), Category("IntervalComparer<T>.StrictlyContains")]
        public void Contains_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.StrictlyContains(j), expected);
        }

        public static object[] ContainsCases = new object[] {
                new object[] { P, I[0], Is.False},
                new object[] { P, I[1], Is.False},
                new object[] { P, I[2], Is.False},
                new object[] { P, I[3], Is.False},
                new object[] { I[0], P, Is.False},
                new object[] { I[1], P, Is.False},
                new object[] { I[2], P, Is.False},
                new object[] { I[3], P, Is.False}
            };
    }

    /// <summary>
    /// P starts I
    /// P
    /// IIIIII
    /// </summary>
    [TestFixture]
    public class IntervalPStartsX
    {
        private static readonly IInterval<int> P = new IntervalBase<int>(0);
        private static readonly IInterval<int>[] I = new[] {
                new IntervalBase<int>(0, 5, false, false), // ()
                new IntervalBase<int>(0, 5, false, true), // (]
                new IntervalBase<int>(0, 5, true, false), // [)
                new IntervalBase<int>(0, 5, true, true) // []
            };

        [Test]
        public void Overlap()
        {
            Assert.False(I[0].Overlaps(P));
            Assert.False(I[1].Overlaps(P));
            Assert.That(I[2].Overlaps(P));
            Assert.That(I[3].Overlaps(P));

            Assert.False(P.Overlaps(I[0]));
            Assert.False(P.Overlaps(I[1]));
            Assert.That(P.Overlaps(I[2]));
            Assert.That(P.Overlaps(I[3]));
        }

        //Sorting
        [TestCaseSource(typeof(IntervalPStartsX), "StabCases")]
        public void Sorting_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.CompareTo(j), expected);
        }

        public static object[] StabCases = new object[] {
                new object[] { I[0], P, Is.GreaterThan(0)},
                new object[] { I[1], P, Is.GreaterThan(0)},
                new object[] { I[2], P, Is.GreaterThan(0)},
                new object[] { I[3], P, Is.GreaterThan(0)},
                new object[] { P, I[0], Is.LessThan(0)},
                new object[] { P, I[1], Is.LessThan(0)},
                new object[] { P, I[2], Is.LessThan(0)},
                new object[] { P, I[3], Is.LessThan(0)}
            };

        [Test]
        public void Contains()
        {
            Assert.False(P.StrictlyContains(I[0]));
            Assert.False(P.StrictlyContains(I[1]));
            Assert.False(P.StrictlyContains(I[2]));
            Assert.False(P.StrictlyContains(I[3]));

            Assert.False(I[0].StrictlyContains(P));
            Assert.False(I[1].StrictlyContains(P));
            Assert.False(I[2].StrictlyContains(P));
            Assert.False(I[3].StrictlyContains(P));
        }
    }

    /// <summary>
    /// P overlaps I
    ///      P
    /// IIIIIIIIIII
    /// </summary>
    [TestFixture]
    public class IntervalPOverlapsX
    {
        private static readonly IInterval<int> P = new IntervalBase<int>(5);
        private static readonly IInterval<int>[] I = new[] {
                new IntervalBase<int>(0, 10, false, false), // ()
                new IntervalBase<int>(0, 10, false, true), // (]
                new IntervalBase<int>(0, 10, true, false), // [)
                new IntervalBase<int>(0, 10, true, true) // []
            };

        [Test]
        public void Overlap()
        {
            Assert.That(I[0].Overlaps(P));
            Assert.That(I[1].Overlaps(P));
            Assert.That(I[2].Overlaps(P));
            Assert.That(I[3].Overlaps(P));

            Assert.That(P.Overlaps(I[0]));
            Assert.That(P.Overlaps(I[1]));
            Assert.That(P.Overlaps(I[2]));
            Assert.That(P.Overlaps(I[3]));
        }

        //Sorting
        [TestCaseSource(typeof(IntervalPOverlapsX), "StabCases")]
        public void Sorting_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.CompareTo(j), expected);
        }

        public static object[] StabCases = new object[] {
                new object[] { I[0], P, Is.LessThan(0)},
                new object[] { I[1], P, Is.LessThan(0)},
                new object[] { I[2], P, Is.LessThan(0)},
                new object[] { I[3], P, Is.LessThan(0)},
                new object[] { P, I[0], Is.GreaterThan(0)},
                new object[] { P, I[1], Is.GreaterThan(0)},
                new object[] { P, I[2], Is.GreaterThan(0)},
                new object[] { P, I[3], Is.GreaterThan(0)}
            };

        [Test]
        public void Contains()
        {
            Assert.False(P.StrictlyContains(I[0]));
            Assert.False(P.StrictlyContains(I[1]));
            Assert.False(P.StrictlyContains(I[2]));
            Assert.False(P.StrictlyContains(I[3]));

            Assert.That(I[0].StrictlyContains(P));
            Assert.That(I[1].StrictlyContains(P));
            Assert.That(I[2].StrictlyContains(P));
            Assert.That(I[3].StrictlyContains(P));
        }
    }

    /// <summary>
    /// P finishes X
    ///      P
    /// XXXXXX
    /// </summary>
    [TestFixture]
    public class IntervalPFinishesX
    {
        private static readonly IInterval<int> P = new IntervalBase<int>(5);
        private static readonly IInterval<int>[] I = new[] {
                new IntervalBase<int>(0, 5, false, false), // ()
                new IntervalBase<int>(0, 5, false, true), // (]
                new IntervalBase<int>(0, 5, true, false), // [)
                new IntervalBase<int>(0, 5, true, true), // []
            };

        [TestCaseSource("OverlapCases"), Category("IntervalComparer<T>.Overlaps")]
        public void Overlap_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.Overlaps(j), expected);
        }

        public static object[] OverlapCases = new object[] {
                new object[] { I[0], P, Is.False},
                new object[] { I[1], P, Is.True},
                new object[] { I[2], P, Is.False},
                new object[] { I[3], P, Is.True},
                new object[] { P, I[0], Is.False},
                new object[] { P, I[1], Is.True},
                new object[] { P, I[2], Is.False},
                new object[] { P, I[3], Is.True}
            };


        // Sorting
        [TestCaseSource(typeof(IntervalPFinishesX), "StabCases")]
        public void Sorting_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.CompareTo(j), expected);
        }
        public static object[] StabCases = new object[] {
                new object[] { I[0], P, Is.LessThan(0)},
                new object[] { I[1], P, Is.LessThan(0)},
                new object[] { I[2], P, Is.LessThan(0)},
                new object[] { I[3], P, Is.LessThan(0)},
                new object[] { P, I[0], Is.GreaterThan(0)},
                new object[] { P, I[1], Is.GreaterThan(0)},
                new object[] { P, I[2], Is.GreaterThan(0)},
                new object[] { P, I[3], Is.GreaterThan(0)},
            };

        [Test]
        public void Contains()
        {
            Assert.False(P.StrictlyContains(I[0]));
            Assert.False(P.StrictlyContains(I[1]));
            Assert.False(P.StrictlyContains(I[2]));
            Assert.False(P.StrictlyContains(I[3]));
            Assert.False(I[0].StrictlyContains(P));
            Assert.False(I[1].StrictlyContains(P));
            Assert.False(I[2].StrictlyContains(P));
            Assert.False(I[3].StrictlyContains(P));
        }
    }

    /// <summary>
    /// P after I
    ///           P
    /// IIIIII
    /// </summary>
    [TestFixture]
    public class IntervalPAfterX
    {
        private static readonly IInterval<int> P = new IntervalBase<int>(10);
        private static readonly IInterval<int>[] I = new[] {
                new IntervalBase<int>(0, 5, false, false), // ()
                new IntervalBase<int>(0, 5, false, true), // (]
                new IntervalBase<int>(0, 5, true, false), // [)
                new IntervalBase<int>(0, 5, true, true) // []
            };

        [Test, Combinatorial, Category("IntervalComparer<T>.Overlaps")]
        public void Overlaps_After_Combitorial([ValueSource("I")] IInterval<int> i, [Values(true, false)] bool swap)
        {
            Assert.IsFalse(swap ? P.Overlaps(i) : i.Overlaps(P));
        }

        [Test, Combinatorial, Category("IntervalComparer<T>.StaticCompare")]
        public void StaticCompare_After_Combitorial([ValueSource("I")] IInterval<int> i, [Values(true, false)] bool swap)
        {
            if (swap)
                Assert.Greater(P.CompareTo(i), 0);
            else
                Assert.Less(i.CompareTo(P), 0);
        }

        [Test, Combinatorial, Category("IntervalComparer<T>.StrictlyContains")]
        public void Contains_After_Combitorial([ValueSource("I")] IInterval<int> i, [Values(true, false)] bool swap)
        {
            Assert.IsFalse(swap ? P.StrictlyContains(i) : i.StrictlyContains(P));
        }


        private IInterval<int>[] BenchmarkIntervals()
        {
            var intervals = new ArrayList<IInterval<int>>();

            var size = 10;

            for (int low = 0; low < size; low++)
            {
                intervals.Add(new IntervalBase<int>(low));

                for (int high = low + 1; high < size; high++)
                {
                    for (int lowInc = 0; lowInc < 2; lowInc++)
                    {
                        var lowIncluded = Convert.ToBoolean(lowInc);
                        for (int highInc = 0; highInc < 2; highInc++)
                        {
                            var highIncluded = Convert.ToBoolean(highInc);

                            intervals.Add(new IntervalBase<int>(low, high, lowIncluded, highIncluded));
                        }
                    }
                }
            }

            return intervals.ToArray();
        }
    }
    // ReSharper restore CoVariantArrayConversion

}
