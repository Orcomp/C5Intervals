using System;
using System.Diagnostics.Contracts;

namespace C5.Intervals
{
    public static class IntervalTypeExtensions
    {
        public static IntervalType IntervalType<T>(this IInterval<T> interval) where T : IComparable<T>
        {
            Contract.Requires(interval != null);
            Contract.Ensures((Contract.Result<IntervalType>() == Intervals.IntervalType.Closed) == interval.IsClosed());
            Contract.Ensures((Contract.Result<IntervalType>() == Intervals.IntervalType.Open) == interval.IsOpen());

            return (IntervalType) ((interval.LowIncluded ? 2 : 0) + (interval.HighIncluded ? 1 : 0));
        }

        public static bool IsOpen<T>(this IInterval<T> interval) where T : IComparable<T>
        {
            Contract.Requires(interval != null);

            return !interval.LowIncluded && !interval.HighIncluded;
        }

        public static bool IsClosed<T>(this IInterval<T> interval) where T : IComparable<T>
        {
            Contract.Requires(interval != null);

            return interval.LowIncluded && interval.HighIncluded;
        }
    }
}
