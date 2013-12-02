﻿using C5.intervals;

namespace C5.Performance.Wpf.Benchmarks
{
    public class IbsAvlAddBenchmarker : Benchmarkable
    {
        private IntervalBinarySearchTreeAvl<IInterval<int>, int> _collection;
        private IInterval<int>[] _intervals;

        public override void CollectionSetup()
        {
            _intervals = Tests.intervals.BenchmarkTestCases.DataSetC(CollectionSize);
            _collection = new IntervalBinarySearchTreeAvl<IInterval<int>, int>();
            ItemsArray = SearchAndSort.FillIntArray(CollectionSize);
            SearchAndSort.Shuffle(ItemsArray);
        }

        public override void Setup() { }

        public override double Call(int i)
        {
            foreach (var interval in _intervals)
                _collection.Add(interval);

            return _collection.Count;
        }

        public override string BenchMarkName()
        {
            return "IBS Add (AVL)";
        }
    }
}
