using System;

namespace C5.Intervals.Tests
{
    namespace StaticFiniteIntervalList
    {
        #region Black-box

        class StaticFiniteIntervalListTester_BlackBox : IntervalCollectionTester
        {
            protected override Type GetCollectionType()
            {
                return typeof(StaticFiniteIntervalList<,>);
            }

            protected override Speed CountSpeed()
            {
                return Speed.Constant;
            }

            protected override bool AllowsReferenceDuplicates()
            {
                return false;
            }

            protected override object[] AdditionalParameters()
            {
                return new object[] { true };
            }
        }

        #endregion

        #region White-box

        #endregion
    }
}
