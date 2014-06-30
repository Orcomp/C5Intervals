using System;
using C5.Intervals;

namespace C5.UserGuideExamples.Intervals
{
    public class IntervalExtensionsExamples
    {
        public static void Main(string[] args)
        {
            var x = new IntervalBase<int>(1, 5, IntervalType.LowIncluded);  // [1:5)
            var y = new IntervalBase<int>(2, 3, IntervalType.Closed);       // [2:3]
            var z = new IntervalBase<int>(3, 5, IntervalType.Open);         // (3:5)
            var p = new IntervalBase<int>(3);                               // [3:3]

            var a = new IntervalBase<int>(1, 5, IntervalType.LowIncluded); // [1:5)
            var b = new IntervalBase<int>(3, 1, IntervalType.Open); // (3:1) - invalid interval

            var intervals = new[] { x, y, z, p };

            Console.Out.WriteLine("x.Overlaps(y): {0}", x.Overlaps(y)); // true
            Console.Out.WriteLine("y.Overlaps(x): {0}", y.Overlaps(x)); // true
            Console.Out.WriteLine("y.Overlaps(z): {0}\n", y.Overlaps(z)); // false

            Console.Out.WriteLine("y.Overlaps(3): {0}", y.Overlaps(3)); // true
            Console.Out.WriteLine("z.Overlaps(3): {0}\n", z.Overlaps(3)); // false

            Console.Out.WriteLine("x.Contains(y): {0}", x.Contains(y)); // true
            Console.Out.WriteLine("y.Contains(x): {0}", y.Contains(x)); // false
            Console.Out.WriteLine("x.Contains(z): {0}\n", x.Contains(z)); // true

            Console.Out.WriteLine("x.StrictlyContains(y): {0}", x.StrictlyContains(y)); // true
            Console.Out.WriteLine("x.StrictlyContains(z): {0}\n", x.StrictlyContains(z)); // false

            Console.Out.WriteLine("x.CompareTo(y): {0}", x.CompareTo(y)); // -1
            Console.Out.WriteLine("y.CompareTo(x): {0}\n", y.CompareTo(x)); // 1

            Console.Out.WriteLine("x.CompareLow(y): {0}", x.CompareLow(y)); // -1
            Console.Out.WriteLine("z.CompareLow(y): {0}\n", z.CompareLow(y)); // 1
            Console.Out.WriteLine("z.CompareLow(z): {0}\n", z.CompareLow(z)); // 0

            Console.Out.WriteLine("x.CompareHigh(y): {0}", x.CompareHigh(y)); // 1
            Console.Out.WriteLine("x.CompareHigh(z): {0}\n", x.CompareHigh(z)); // 0

            Console.Out.WriteLine("x.IntervalEquals(y): {0}", x.IntervalEquals(y)); // false
            Console.Out.WriteLine("x.IntervalEquals(x): {0}", x.IntervalEquals(x)); // true
            Console.Out.WriteLine("x.IntervalEquals(a): {0}\n", x.IntervalEquals(a)); // true

            Console.Out.WriteLine("x.Overlap(y): {0}", x.Overlap(y)); // [2:3]
            Console.Out.WriteLine("x.Overlap(z): {0}\n", x.Overlap(z)); // (3:5)
            //Console.Out.WriteLine("y.Overlap(z): {0}", y.Overlap(z)); // Exception - no overlap

            Console.Out.WriteLine("x.JoinedSpan(y): {0}", x.JoinedSpan(y)); // [1:5)
            Console.Out.WriteLine("y.JoinedSpan(z): {0}\n", y.JoinedSpan(z)); // [2:5)

            Console.Out.WriteLine("y.HighestHigh(z): {0}", y.HighestHigh(z)); // (3:5)
            Console.Out.WriteLine("y.HighestHigh(p): {0}\n", y.HighestHigh(p)); // [2:3] or [3:3]

            Console.Out.WriteLine("x.LowestLow(y): {0}", x.LowestLow(y)); // [1:5)
            Console.Out.WriteLine("z.LowestLow(y): {0}\n", z.LowestLow(y)); // [2:3]

            Console.Out.WriteLine("x.IsValidInterval(): {0}", x.IsValidInterval()); // true
            Console.Out.WriteLine("z.IsValidInterval(): {0}", z.IsValidInterval()); // true
            Console.Out.WriteLine("b.IsValidInterval(): {0}\n", b.IsValidInterval()); // false

            Console.Out.WriteLine("x.IsPoint(): {0}", x.IsPoint()); // false
            Console.Out.WriteLine("p.IsPoint(): {0}\n", p.IsPoint()); // true

            Console.Out.WriteLine("x.GetIntervalHashCode(): {0}", x.GetIntervalHashCode()); // 15730764
            Console.Out.WriteLine("a.GetIntervalHashCode(): {0}", a.GetIntervalHashCode()); // 15730764
            Console.Out.WriteLine("y.GetIntervalHashCode(): {0}", y.GetIntervalHashCode()); // 15760494
            Console.Out.WriteLine("z.GetIntervalHashCode(): {0}\n", z.GetIntervalHashCode()); // 15789385

            Console.Out.WriteLine("x.ToIntervalString(): {0}", x.ToIntervalString()); // [1:5)
            Console.Out.WriteLine("y.ToIntervalString(): {0}", y.ToIntervalString()); // [2:3]
            Console.Out.WriteLine("z.ToIntervalString(): {0}", z.ToIntervalString()); // (3:5)
            Console.Out.WriteLine("p.ToIntervalString(): {0}\n", p.ToIntervalString()); // (3:5)

            Console.Out.WriteLine("intervals.UniqueEndpointValues(): {0}\n", String.Join(", ", intervals.UniqueEndpointValues())); // 1, 2, 3, 5

            Console.Out.WriteLine("intervals.Span(): {0}\n", intervals.Span()); // [1:5)

            IInterval<int> intervalOfMaximumDepth = null;
            Console.Out.WriteLine("intervals.MaximumDepth(ref intervalOfMaximumDepth, false): {0}", intervals.MaximumDepth(ref intervalOfMaximumDepth, false)); // 3
            Console.Out.WriteLine("intervalOfMaximumDepth: {0}", intervalOfMaximumDepth); // [3:3]


            Console.Read();
        }
    }
}
