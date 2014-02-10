using System;
using System.Collections.Generic;
using System.Linq;
using C5.Intervals;

namespace C5.UserGuideExamples.Intervals
{
    /// <summary>
    /// The example shows how to use the extension methods from <see cref="C5.Intervals"/> to solve
    /// the dynamic programming problem of finding the biggest set of non-overlapping intervals with the
    /// highest weight sum.
    /// </summary>
    public class WeigthedIntervalScheduling
    {
        public static void Main(String[] args)
        {
            // Intervals (here sorted by weight)
            var intervals = new[] {
                    new WeightedInterval(1.0, 10, 13),
                    new WeightedInterval(2.0,  1,  4),
                    new WeightedInterval(2.0,  9, 12),
                    new WeightedInterval(4.0,  5,  8),
                    new WeightedInterval(4.0,  2,  7),
                    new WeightedInterval(7.0,  3, 11)
                };

            var result = CalculateOptimalSolution(intervals);

            Console.Out.Write("The maximum weighted set ({0}) of the intervals is: {1}\n", result.Key, result.Value);
            Console.Read();
        }

        public static KeyValuePair<double, IEnumerable<WeightedInterval>> CalculateOptimalSolution(IEnumerable<WeightedInterval> intervalEnumerable)
        {
            // Make intervals to array to allow fast sorting and counting
            var intervals = intervalEnumerable as WeightedInterval[] ?? intervalEnumerable.ToArray();

            // Return if no intervals were given
            if (!intervals.Any())
                return new KeyValuePair<double, IEnumerable<WeightedInterval>>(0.0, Enumerable.Empty<WeightedInterval>());

            var count = intervals.Length;

            // Sort intervals on high then low endpoint
            var comparer = ComparerFactory<WeightedInterval>.CreateComparer((x, y) =>
                {
                    var compare = x.CompareHigh(y);
                    return compare != 0 ? compare : x.CompareLow(y);
                });
            Sorting.IntroSort(intervals, 0, count, comparer);

            // Calculate the previous non-overlapping interval for all intervals
            var p = findP(intervals);

            // Iteratively calculate the solution for each subproblem
            var opt = new double[count];
            var includeInterval = new bool[count];
            for (var i = 0; i < count; i++)
            {
                var included = intervals[i].Weight + (p[i] < 0 ? 0 : opt[p[i]]);
                var notIncluded = i - 1 < 0 ? 0 : opt[i - 1];

                var include = included >= notIncluded;

                opt[i] = include ? included : notIncluded;
                includeInterval[i] = include;
            }

            // Back-trace the solution
            var set = new ArrayList<WeightedInterval>();
            for (var i = count - 1; i >= 0; )
            {
                if (includeInterval[i])
                {
                    set.Add(intervals[i]);
                    i = p[i];
                }
                else
                    i--;
            }

            return new KeyValuePair<double, IEnumerable<WeightedInterval>>(opt[count - 1], set);
        }

        private static int[] findP(WeightedInterval[] intervals)
        {
            var p = new int[intervals.Length];

            // Create queue sorted on low endpoints
            var comparer = ComparerFactory<KeyValuePair<IInterval<int>, int>>.CreateComparer((x, y) => y.Key.CompareLow(x.Key));
            var queue = new IntervalHeap<KeyValuePair<IInterval<int>, int>>(comparer);

            for (var i = intervals.Length - 1; i >= 0; i--)
            {
                var interval = intervals[i];

                // Remove all intervals from the queue not overlapping the current interval
                while (!queue.IsEmpty && interval.CompareHighLow(queue.FindMin().Key) < 0)
                    // Save the index for the non-overlapping interval
                    p[queue.DeleteMin().Value] = i;

                queue.Add(new KeyValuePair<IInterval<int>, int>(interval, i));
            }

            // The remaining intervals in the queue all overlap the previous intervals
            while (!queue.IsEmpty)
                p[queue.DeleteMin().Value] = -1;

            return p;
        }

        /// <summary>
        /// An interval with a floating point weight as implementation of <see cref="IInterval{T}"/>.
        /// </summary>
        public class WeightedInterval : IInterval<int>
        {
            public WeightedInterval(double weight, int low, int high)
            {
                Weight = weight;
                High = high;
                Low = low;
            }

            public int Low { get; private set; }
            public int High { get; private set; }
            public bool LowIncluded { get { return true; } }
            public bool HighIncluded { get { return true; } }
            public double Weight { get; private set; }

            public override string ToString()
            {
                return String.Format("{0} ({1})", this.ToIntervalString(), Weight);
            }
        }
    }
}
