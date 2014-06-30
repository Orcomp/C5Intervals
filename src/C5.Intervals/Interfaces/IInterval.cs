using System;
using System.Diagnostics.Contracts;

namespace C5.Intervals
{
    /// <summary>
    /// An interval with included or excluded low and high endpoints.
    /// </summary>
    /// <remarks>None of the values should be changed once the interval is stored in an interval collection, as it most certainly will break the data structure.</remarks>
    /// <typeparam name="T">The generic type of the interval's endpoint values.</typeparam>
    [ContractClass(typeof(IntervalContract<>))]
    public interface IInterval<T> where T : IComparable<T>
    {
        // @design: shortest and most generic terms for endpoints
        /// <summary>
        /// Get the low endpoint for the interval.
        /// </summary>
        /// <value>The low endpoint value.</value>
        /// <remarks>Also known as left, start, first, or lower endpoint.</remarks>
        [Pure]
        T Low { get; }

        /// <summary>
        /// Get the high endpoint for the interval.
        /// </summary>
        /// <value>The high endpoint value.</value>
        /// <remarks>Also known as right, end, stop, last, or higher endpoint.</remarks>
        [Pure]
        T High { get; }

        /// <summary>
        /// Get the low endpoint's inclusion.
        /// </summary>
        /// <value>True if the low endpoint is included in the interval, otherwise false.</value>
        /// <remarks>A stabbing query using the low endpoint value will not include the interval, if the value is false.</remarks>
        [Pure]
        bool LowIncluded { get; }

        /// <summary>
        /// Get the high endpoint's inclusion.
        /// </summary>
        /// <value>True if the high endpoint is included in the interval, otherwise false.</value>
        /// <remarks>A stabbing query using the high endpoint value will not include the interval, if the value is false.</remarks>
        [Pure]
        bool HighIncluded { get; }
    }

    [ContractClassFor(typeof(IInterval<>))]
    internal abstract class IntervalContract<T> : IInterval<T> where T : IComparable<T>
    {
        public T Low
        {
            // TODO: Check these actually work
            get
            {
                Contract.Ensures(Contract.Result<T>() != null);
                Contract.Ensures(Contract.Result<T>().CompareTo(High) < 0 || (Contract.Result<T>().CompareTo(High) == 0 && LowIncluded && HighIncluded));

                throw new NotImplementedException();
            }
        }

        public T High
        {
            get
            {
                Contract.Ensures(Contract.Result<T>() != null);
                Contract.Ensures(Low.CompareTo(Contract.Result<T>()) < 0 || (Low.CompareTo(Contract.Result<T>()) == 0 && LowIncluded && HighIncluded));

                throw new NotImplementedException();
            }
        }

        public bool LowIncluded { get; private set; }
        public bool HighIncluded { get; private set; }
    }
}