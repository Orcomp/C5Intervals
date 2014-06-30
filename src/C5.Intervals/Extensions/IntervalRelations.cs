using System;
using System.Diagnostics.Contracts;

namespace C5.Intervals
{
    /// <summary>
    /// The relationship between two intervals as described by James F. Allen in "Maintaining Knowledge about Temporal Intervals"
    /// </summary>
    public enum IntervalRelation
    {
        /// <summary>
        /// The interval is after another interval.
        /// </summary>
        [Symbol(">")]
        After = 0,

        /// <summary>
        /// The interval is met by another interval. They do not overlap and there is no gap between them.
        /// </summary>
        [Symbol("mi")]
        MetBy = 1,

        /// <summary>
        /// The interval is overlapped by another interval.
        /// </summary>
        [Symbol("oi")]
        OverlappedBy = 2,

        /// <summary>
        /// The interval finishes another interval thereby sharing the high endpoint
        /// </summary>
        [Symbol("f")]
        Finishes = 3,

        /// <summary>
        /// The interval is during another interval.
        /// </summary>
        [Symbol("d")]
        During = 4,

        /// <summary>
        /// The interval is started by another interval thereby sharing the low endpoint.
        /// </summary>
        [Symbol("si")]
        StartedBy = 5,

        /// <summary>
        /// The interval is equal to another interval thereby sharing both endpoints.
        /// </summary>
        [Symbol("e")]
        Equals = 6,

        /// <summary>
        /// The interval is starts another interval thereby sharing the low endpoint.
        /// </summary>
        [Symbol("s")]
        Starts = 7,

        /// <summary>
        /// The interval is contained in another interval.
        /// </summary>
        [Symbol("di")]
        Contains = 8,

        /// <summary>
        /// The interval finished by another interval thereby sharing the high endpoint.
        /// </summary>
        [Symbol("fi")]
        FinishedBy = 9,

        /// <summary>
        /// The interval overlaps another interval.
        /// </summary>
        [Symbol("o")]
        Overlaps = 10,

        /// <summary>
        /// The interval meets another interval. They do not overlap and there is no gap between them.
        /// </summary>
        [Symbol("m")]
        Meets = 11,

        /// <summary>
        /// The interval is before another interval.
        /// </summary>
        [Symbol("<")]
        Before = 12,
    };

    /// <summary>
    /// An extension class for finding the relation between two intervals
    /// </summary>
    public static class IntervalRelations
    {

        /*public static IEnumerable<IntervalRelation> TrasitivityRelations<T>(IntervalRelation xToY, IntervalRelation yToZ)
            where T : IComparable<T>
        {
            // Return possible relations between x and z

            throw new NotImplementedException();
        }*/

        /// <summary>
        /// Get the relationship between two intervals as described by James F. Allen in "Maintaining Knowledge about Temporal Intervals".
        /// The logic for points is described by Marc B. Vilain in "A System for Reasoning About Time", though terms are still Allen.
        /// </summary>
        /// <param name="x">The first interval</param>
        /// <param name="y">The second interval</param>
        /// <returns>How x relates to y</returns>
        public static IntervalRelation RelateTo<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);

            Contract.Ensures((Contract.Result<IntervalRelation>() == IntervalRelation.After) == x.IsAfter(y));
            Contract.Ensures((Contract.Result<IntervalRelation>() == IntervalRelation.MetBy) == x.IsMetBy(y));
            Contract.Ensures(Contract.Result<IntervalRelation>() != IntervalRelation.OverlappedBy || y.CompareLow(x) < 0 && y.CompareHighLow(x) >= 0 && y.CompareHigh(x) < 0);
            Contract.Ensures(Contract.Result<IntervalRelation>() != IntervalRelation.Finishes || y.CompareLow(x) < 0 && y.CompareHighLow(x) >= 0 && y.CompareHigh(x) == 0);
            Contract.Ensures(Contract.Result<IntervalRelation>() != IntervalRelation.During || y.CompareLow(x) < 0 && y.CompareHighLow(x) > 0 && y.CompareHigh(x) > 0);
            Contract.Ensures(Contract.Result<IntervalRelation>() != IntervalRelation.StartedBy || y.CompareLow(x) == 0 && y.CompareHighLow(x) >= 0 && y.CompareHigh(x) < 0);
            Contract.Ensures(Contract.Result<IntervalRelation>() != IntervalRelation.Equals || y.CompareLow(x) == 0 && y.CompareHigh(x) == 0);
            Contract.Ensures(Contract.Result<IntervalRelation>() != IntervalRelation.Starts || y.CompareLow(x) == 0 && y.CompareHigh(x) > 0);
            Contract.Ensures(Contract.Result<IntervalRelation>() != IntervalRelation.Contains || y.CompareLow(x) > 0 && y.CompareHigh(x) < 0);
            Contract.Ensures(Contract.Result<IntervalRelation>() != IntervalRelation.FinishedBy || y.CompareLow(x) > 0 && y.CompareLowHigh(x) <= 0 && y.CompareHigh(x) == 0);
            Contract.Ensures(Contract.Result<IntervalRelation>() != IntervalRelation.Overlaps || y.CompareLow(x) > 0 && x.CompareHighLow(y) >= 0 && y.CompareHigh(x) > 0);
            Contract.Ensures((Contract.Result<IntervalRelation>() == IntervalRelation.Meets) == x.IsMeeting(y));
            Contract.Ensures((Contract.Result<IntervalRelation>() == IntervalRelation.Before) == x.IsBefore(y));


            var compareBefore = x.High.CompareTo(y.Low);
            // Check if x is before y
            if (compareBefore < 0 || compareBefore == 0 && !x.HighIncluded && !y.LowIncluded)
                return IntervalRelation.Before;
            // Check if x meets y
            // Neither do they overlap nor is there a gap between them
            if (compareBefore == 0 && x.HighIncluded != y.LowIncluded)
                return IntervalRelation.Meets;

            // Check if x is after y
            var compareAfter = y.High.CompareTo(x.Low);
            if (compareAfter < 0 || compareAfter == 0 && !y.HighIncluded && !x.LowIncluded)
                return IntervalRelation.After;
            // Check if x is met by y
            if (compareAfter == 0 && y.HighIncluded != x.LowIncluded)
                return IntervalRelation.MetBy;

            var compareLow = x.CompareLow(y);
            var compareHigh = x.CompareHigh(y);

            // Convert the compared values to the integer values of the interval relations
            return (IntervalRelation)
                ((compareLow > 0 ? 2 : (compareLow == 0 ? 5 : 8)) +
                (compareHigh > 0 ? 0 : (compareHigh == 0 ? 1 : 2)));
        }

        // TODO: Optimize to check for just the specified relation
        [Pure]
        public static bool IsAfter<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Ensures(Contract.Result<bool>() == y.IsBefore(x));

            var compare = y.High.CompareTo(x.Low);
            return compare < 0 || compare == 0 && !y.HighIncluded && !x.LowIncluded;
        }

        [Pure]
        public static bool IsMetBy<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Ensures(Contract.Result<bool>() == y.IsMeeting(x));

            return y.High.CompareTo(x.Low) == 0 && y.HighIncluded != x.LowIncluded;
        }

        [Pure]
        public static bool IsOverlappedBy<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Ensures(Contract.Result<bool>() == y.IsOverlapping(x));

            return x.RelateTo(y) == IntervalRelation.OverlappedBy;
        }

        [Pure]
        public static bool IsFinishing<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Ensures(Contract.Result<bool>() == y.IsFinishedBy(x));

            return x.RelateTo(y) == IntervalRelation.Finishes;
        }

        [Pure]
        public static bool IsDuring<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Ensures(Contract.Result<bool>() == y.IsContaining(x));

            return x.RelateTo(y) == IntervalRelation.During;
        }

        [Pure]
        public static bool IsStartedBy<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Ensures(Contract.Result<bool>() == y.IsStarting(x));

            return x.RelateTo(y) == IntervalRelation.StartedBy;
        }

        [Pure]
        public static bool IsEqualTo<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Ensures(Contract.Result<bool>() == y.IsEqualTo(x));

            return x.IntervalEquals(y);
        }

        [Pure]
        public static bool IsStarting<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Ensures(Contract.Result<bool>() == y.IsStartedBy(x));

            return x.RelateTo(y) == IntervalRelation.Starts;
        }

        [Pure]
        public static bool IsContaining<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Ensures(Contract.Result<bool>() == y.IsDuring(x));

            return x.RelateTo(y) == IntervalRelation.Contains;
        }

        [Pure]
        public static bool IsFinishedBy<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Ensures(Contract.Result<bool>() == y.IsFinishing(x));

            return x.RelateTo(y) == IntervalRelation.FinishedBy;
        }

        [Pure]
        public static bool IsOverlapping<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Ensures(Contract.Result<bool>() == y.IsOverlappedBy(x));

            return x.RelateTo(y) == IntervalRelation.Overlaps;
        }

        [Pure]
        public static bool IsMeeting<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Ensures(Contract.Result<bool>() == y.IsMetBy(x));

            return x.High.CompareTo(y.Low) == 0 && x.HighIncluded != y.LowIncluded;
        }

        [Pure]
        public static bool IsBefore<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Ensures(Contract.Result<bool>() == y.IsAfter(x));

            var compare = x.High.CompareTo(y.Low);
            return compare < 0 || compare == 0 && !x.HighIncluded && !y.LowIncluded;
        }
    }

    /// <summary>
    /// Attribute class for the symbol annotation for interval relations
    /// </summary>
    public class SymbolAttribute : Attribute
    {
        /// <summary>
        /// The symbol name
        /// </summary>
        public string S;

        /// <summary>
        /// Create a symbol with the given string
        /// </summary>
        /// <param name="s">The symbol name.</param>
        public SymbolAttribute(string s)
        {
            Contract.Requires(s != null);

            S = s;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return S;
        }
    }

    /// <summary>
    /// A reflection helper class to get the symbol name for an interval relation.
    /// </summary>
    public static class ReflectionHelpers
    {
        /// <summary>
        /// Get the symbol name for a interval relation
        /// </summary>
        /// <param name="objEnum"></param>
        /// <returns></returns>
        public static string GetCustomDescription(object objEnum)
        {
            var attributes = (SymbolAttribute[]) objEnum.GetType().GetField(objEnum.ToString()).GetCustomAttributes(typeof(SymbolAttribute), false);
            return (attributes.Length > 0) ? attributes[0].ToString() : objEnum.ToString();
        }

        /// <summary>
        /// Get the symbol name for an interval relation.
        /// </summary>
        /// <param name="value">The interval relation.</param>
        /// <returns>A string representation of an interval relation.</returns>
        public static string Symbol(this IntervalRelation value)
        {
            return GetCustomDescription(value);
        }
    }
}
