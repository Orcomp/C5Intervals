﻿using System.Linq;
using C5.intervals;
using C5.Tests.intervals;

namespace C5.Performance.Wpf.Benchmarks
{
    public class DITSearchRecursiveBenchmark : Benchmarkable
    {
        private IInterval<int>[] _intervals;
        private IInterval<int>[] _intervalsNot;
        private DynamicIntervalTree<IInterval<int>, int> _intervalCollection; 

        private int intervalSearch(int intervalId)
        {
            if (intervalId < CollectionSize)
                return _intervalCollection.FindOverlapsRecursive(_intervals[intervalId]).Count() > 0 ? 1 : 0;
            return _intervalCollection.FindOverlapsRecursive(_intervalsNot[intervalId - CollectionSize]).Count() > 0 ? 1 : 0;
        }

        public override void CollectionSetup()
        {
            _intervals = BenchmarkTestCases.DataSetB(CollectionSize);
            _intervalsNot = BenchmarkTestCases.DataSetNotA(CollectionSize);
            _intervalCollection = new DynamicIntervalTree<IInterval<int>, int>(_intervals);

            /*
             * Setup an items array with things to look for.
             * Fill in random numbers from 0 to the number of trains plus the number of trains not in the collection.
             * This should make roughly half the searched succesful if we find enough space to generate as many trains not in the collection as there is trains already.
             */
            ItemsArray = SearchAndSort.FillIntArrayRandomly(CollectionSize, 0, CollectionSize * 2);
        }

        public override void Setup()
        {
        }

        public override double Call(int i)
        {
            return intervalSearch(i);
        }

        public override string BenchMarkName()
        {
            return "DIT Recursive Search";
        }
    }
}