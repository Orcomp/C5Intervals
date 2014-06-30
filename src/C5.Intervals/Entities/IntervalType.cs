using System;

namespace C5.Intervals
{
    /// <summary>
    /// An enum describing how the endpoints should be included.
    /// </summary>
    [Flags]
    public enum IntervalType : byte
    {
        /// <summary>
        /// An interval without the endpoints included.
        /// </summary>
        Open = 0,

        /// <summary>
        /// An interval with the low endpoint included and the high excluded.
        /// </summary>
        LowIncluded = 2,
        /// <summary>
        /// An interval with the low endpoint included and the high excluded.
        /// </summary>
        /// <remarks>Equal to <see cref="LowIncluded"/></remarks>
        LeftClosed = LowIncluded,
        /// <summary>
        /// An interval with the low endpoint included and the high excluded.
        /// </summary>
        /// <remarks>Equal to <see cref="LowIncluded"/></remarks>
        RightOpen = LowIncluded,
        /// <summary>
        /// An interval with the low endpoint included and the high excluded.
        /// </summary>
        /// <remarks>Equal to <see cref="LowIncluded"/></remarks>
        TimeInterval = LowIncluded,

        /// <summary>
        /// An interval with the low endpoint excluded and the high included.
        /// </summary>
        /// <remarks>Equal to <see cref="RightClosed"/></remarks>
        HighIncluded = 1,
        /// <summary>
        /// An interval with the low endpoint excluded and the high included.
        /// </summary>
        RightClosed = HighIncluded,
        /// <summary>
        /// An interval with the low endpoint excluded and the high included.
        /// </summary>
        /// <remarks>Equal to <see cref="RightClosed"/></remarks>
        LeftOpen = HighIncluded,

        /// <summary>
        /// An interval with both endpoints included.
        /// </summary>
        Closed = LowIncluded | HighIncluded
    }
}
