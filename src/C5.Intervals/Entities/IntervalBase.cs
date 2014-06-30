using System;
using System.Diagnostics.Contracts;

namespace C5.Intervals
{
    /// <summary>
    /// Basic interval class with immutable endpoints. Useful for return or query values or as base class for own implementation of <see cref="IInterval{T}"/>.
    /// </summary>
    /// <seealso cref="IInterval{T}"/>
    public class IntervalBase<T> : IInterval<T> where T : IComparable<T>
    {
        #region Fields

        // Use read-only fields to avoid breaking data structures, if values were changed
        /// <inheritdoc/>
        [ContractPublicPropertyName("Low")]
        private T _low;
        /// <inheritdoc/>
        [ContractPublicPropertyName("High")]
        private T _high;
        /// <inheritdoc/>
        [ContractPublicPropertyName("LowIncluded")]
        private bool _lowIncluded;
        /// <inheritdoc/>
        [ContractPublicPropertyName("HighIncluded")]
        private bool _highIncluded;

        /// <inheritdoc/>
        public T Low
        {
            get
            {
                Contract.Ensures(this.IsValidInterval());
                return _low;
            }
            set
            {
                Contract.Requires(value.CompareTo(High) < 0 || value.CompareTo(High) == 0 && LowIncluded && HighIncluded);
                _low = value;
            }
        }

        /// <inheritdoc/>
        public T High
        {
            get
            {
                Contract.Ensures(this.IsValidInterval());
                return _high;
            }
            set
            {
                Contract.Requires(Low.CompareTo(value) < 0 || Low.CompareTo(value) == 0 && LowIncluded && HighIncluded);
                _high = value;
            }
        }

        /// <inheritdoc/>
        public bool LowIncluded
        {
            get
            {
                Contract.Ensures(this.IsValidInterval());
                return _lowIncluded;
            }
            set
            {
                Contract.Requires(Low.CompareTo(High) != 0 || value);
                _lowIncluded = value;
            }
        }

        /// <inheritdoc/>
        public bool HighIncluded
        {
            get
            {
                Contract.Ensures(this.IsValidInterval());
                return _highIncluded;
            }
            set
            {
                Contract.Requires(Low.CompareTo(High) != 0 || value);
                _highIncluded = value;
            }
        }

        public IntervalBase<T> SetLowEndpoint(T low, bool lowIncluded)
        {
            Contract.Requires(low != null);
            // TODO: Consider if contracts should be here
            Contract.Requires(low.CompareTo(High) < 0 || low.CompareTo(High) == 0 && lowIncluded && HighIncluded);

            _low = low;
            _lowIncluded = lowIncluded;

            return this;
        }

        public IntervalBase<T> SetLowEndpoint(IInterval<T> interval)
        {
            Contract.Requires(interval != null);
            // TODO: Consider if contracts should be here
            Contract.Requires(interval.Low.CompareTo(High) < 0 || interval.Low.CompareTo(High) == 0 && interval.LowIncluded && HighIncluded);

            _low = interval.Low;
            _lowIncluded = interval.LowIncluded;

            return this;
        }

        public IntervalBase<T> SetHighEndpoint(T high, bool highIncluded)
        {
            Contract.Requires(high != null);
            // TODO: Consider if contracts should be here
            Contract.Requires(Low.CompareTo(high) < 0 || Low.CompareTo(high) == 0 && LowIncluded && highIncluded);

            _high = high;
            _highIncluded = highIncluded;

            return this;
        }

        public IntervalBase<T> SetHighEndpoint(IInterval<T> interval)
        {
            Contract.Requires(interval != null);
            // TODO: Consider if contracts should be here
            Contract.Requires(Low.CompareTo(interval.High) < 0 || Low.CompareTo(interval.High) == 0 && LowIncluded && interval.HighIncluded);

            _high = interval.High;
            _highIncluded = interval.HighIncluded;

            return this;
        }

        public IntervalBase<T> SetEndpoints(T low, T high, bool? lowIncluded = null, bool? highIncluded = null)
        {
            Contract.Requires(low != null);
            Contract.Requires(high != null);
            Contract.Requires(low.CompareTo(high) < 0 || low.CompareTo(high) == 0 && lowIncluded.GetValueOrDefault(_lowIncluded) && highIncluded.GetValueOrDefault(_highIncluded));

            _low = low;
            _high = high;

            if (lowIncluded != null)
                _lowIncluded = (bool) lowIncluded;
            if (highIncluded != null)
                _highIncluded = (bool) highIncluded;

            return this;
        }

        public IntervalBase<T> SetEndpoints(IInterval<T> low, IInterval<T> high)
        {
            Contract.Requires(low != null);
            Contract.Requires(high != null);
            Contract.Requires(low.Low.CompareTo(high.High) < 0 || low.Low.CompareTo(high.High) == 0 && low.LowIncluded && high.HighIncluded);

            _low = low.Low;
            _lowIncluded = low.LowIncluded;

            _high = high.High;
            _highIncluded = high.HighIncluded;

            return this;
        }

        public IntervalBase<T> SetPoint(T point)
        {
            _low = _high = point;
            _lowIncluded = _highIncluded = true;

            return this;
        }

        public IntervalBase<T> SetInterval(IInterval<T> interval)
        {
            Contract.Requires(interval != null);
            Contract.Requires(interval.IsValidInterval());

            _low = interval.Low;
            _high = interval.High;
            _lowIncluded = interval.LowIncluded;
            _highIncluded = interval.HighIncluded;

            return this;
        }

        public IntervalBase<T> ExpandInterval(IInterval<T> interval)
        {
            Contract.Requires(interval.IsValidInterval());

            if (interval.CompareLow(this) < 0)
                SetLowEndpoint(interval);
            if (this.CompareHigh(interval) < 0)
                SetHighEndpoint(interval);

            return this;
        }

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        private void invariant()
        {
            Contract.Invariant(this.IsValidInterval());
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Make a point with same Low and High and both endpoints included.
        /// </summary>
        /// <param name="query"></param>
        public IntervalBase(T query)
        {
            Contract.Requires(query != null);

            _low = _high = query;
            _lowIncluded = _highIncluded = true;
        }

        /// <summary>
        /// Create an interval. Default to time interval, which has low included and high excluded.
        /// </summary>
        /// <param name="low">Low endpoint.</param>
        /// <param name="high">High endpoint.</param>
        /// <param name="lowIncluded">True if low endpoint is included.</param>
        /// <param name="highIncluded">True if high endpoint is included.</param>
        /// <exception cref="ArgumentException">Thrown if interval is an empty point set.</exception>
        public IntervalBase(T low, T high, bool lowIncluded = true, bool highIncluded = false)
        {
            Contract.Requires(low.CompareTo(high) < 0 || low.CompareTo(high) == 0 && lowIncluded && highIncluded);

            _low = low;
            _high = high;
            _lowIncluded = lowIncluded;
            _highIncluded = highIncluded;
        }

        /// <summary>
        /// Create an interval using <see cref="IntervalType"/> to determine endpoint types. This
        /// can be useful for making code more readable.
        /// </summary>
        /// <param name="low">Low endpoint.</param>
        /// <param name="high">High endpoint.</param>
        /// <param name="type">The interval type.</param>
        /// <exception cref="ArgumentException">Thrown if interval is an empty point set.</exception>
        public IntervalBase(T low, T high, IntervalType type)
        {
            Contract.Requires(low.CompareTo(high) < 0 || low.CompareTo(high) == 0 && type == IntervalType.Closed);

            _low = low;
            _high = high;
            _lowIncluded = (type & IntervalType.LowIncluded) == IntervalType.LowIncluded;
            _highIncluded = (type & IntervalType.HighIncluded) == IntervalType.HighIncluded;
        }

        /// <summary>
        /// Copy the interval data from an <see cref="IInterval&lt;T&gt;"/> to a new interval. 
        /// </summary>
        /// <param name="i"><see cref="IInterval&lt;T&gt;"/> to copy the information from</param>
        public IntervalBase(IInterval<T> i)
        {
            Contract.Requires(i != null);
            Contract.Requires(i.IsValidInterval());

            _low = i.Low;
            _high = i.High;
            _lowIncluded = i.LowIncluded;
            _highIncluded = i.HighIncluded;
        }

        /// <summary>
        /// Create an interval using the low value from one interval, and the high from another interval.
        /// </summary>
        /// <param name="low">The interval from which the low endpoint should be used</param>
        /// <param name="high">The interval from which the high endpoint should be used</param>
        public IntervalBase(IInterval<T> low, IInterval<T> high)
        {
            Contract.Requires(low != null);
            Contract.Requires(high != null);
            Contract.Requires(low.Low.CompareTo(high.High) < 0 || low.Low.CompareTo(high.High) == 0 && low.LowIncluded && high.HighIncluded);

            _low = low.Low;
            _lowIncluded = low.LowIncluded;

            _high = high.High;
            _highIncluded = high.HighIncluded;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public override bool Equals(object that)
        {
            if (ReferenceEquals(null, that)) return false;
            if (ReferenceEquals(this, that)) return true;
            if (GetType() != that.GetType()) return false;
            return IntervalExtensions.IntervalEquals(this, (IInterval<T>) that);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return IntervalExtensions.GetIntervalHashCode(this);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.ToIntervalString();
        }

        #endregion
    }
}
