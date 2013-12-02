using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using C5.Performance.Wpf.Benchmarks;
using Microsoft.Win32;

namespace C5.Performance.Wpf
{
    // Tool for running and plotting benchmarks that are of type Benchmarkable.
    public partial class Benchmarker
    {
        #region Benchmark setup
        // Parameters for running the benchmarks
        private const int MinCollectionSize = 100;
        private const int MaxCollectionSize = 409601;//TrainUtilities.TrainSetACount;
        private const int CollectionMultiplier = 2;
        private const int StandardRepeats = 10;
        private const double MaxExecutionTimeInSeconds = 0.25;
        private readonly Plotter _plotter;
        internal int MaxIterations;
        // Every time we benchmark we count this up in order to get a new color for every benchmark
        private int _lineSeriesIndex;
        private int _maxCount = Int32.MaxValue / 10;
        private int _repeats = StandardRepeats;
        private bool _runSequential;
        private bool _runWarmups = true;

        // These are the benchmarks that will be run by the benchmarker.
        private static Benchmarkable[] Benchmarks
        {
            get
            {
                return new Benchmarkable[]
                {
                    new IbsAvlIntervalSetsBenchmarker(), 
                    new IbsAvlNewIntervalSetsBenchmarker(), 
                };
            }
        }
        #endregion

        #region Constructor
        public Benchmarker()
        {
            MaxIterations = Convert.ToInt32(Math.Round(Math.Log(MaxCollectionSize)));
            _plotter = Plotter.CreatePlotter();
            DataContext = _plotter;
        }
        #endregion

        #region Benchmark Running
        // Method that gets called when the benchmark button is used.
        private void benchmarkStart(object sender, RoutedEventArgs e)
        {
            runSequentialCheckBox.IsEnabled = false;
            CheckBox_Checked_RunQuick(null, null);
            CheckBox_Unchecked_RunWarmups(null,null);

            // This benchmark is the one we use to compare with Sestoft's cmd line version of the tool
            var thread = _runSequential
                ? new Thread(() => runBenchmarks(Benchmarks))
                : new Thread(() => runBenchmarksParallel(Benchmarks));
            //CheckBox checkbox = (CheckBox)this.Controls.Find("checkBox" + input.toString())[0];
            thread.Start();
        }

        // Sequential run of all the benchmarks.
        private void runBenchmarks(params Benchmarkable[] benchmarks)
        {
            //runSequential;
            foreach (var b in benchmarks)
            {
                _plotter.AddAreaSeries(b.BenchMarkName());
                for (b.CollectionSize = MinCollectionSize;
                    b.CollectionSize < MaxCollectionSize;
                    b.CollectionSize *= CollectionMultiplier)
                {
                    updateStatusLabel("Running " + b.BenchMarkName() + " with collection size " + b.CollectionSize);
                    var benchmark = b.Benchmark(_maxCount, _repeats, MaxExecutionTimeInSeconds, this, _runWarmups);
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                        _plotter.AddDataPoint(_lineSeriesIndex, benchmark)));
                    Thread.Sleep(100);
                    updateProgressBar(benchmarks.Length);
                }
                _lineSeriesIndex++;
            }
            UpdateRunningLabel("");
            updateStatusLabel("Finished");
            Thread.Sleep(1000);
            updateStatusLabel("");
        }

        // "Parallel" run of all the benchmarks. Each benchmarkable will get 1 run after another. Making it easier to compare benchmarks as they run.
        private void runBenchmarksParallel(params Benchmarkable[] benchmarks)
        {
            foreach (var benchmarkable in benchmarks)
                _plotter.AddAreaSeries(benchmarkable.BenchMarkName());
            var collectionSize = MinCollectionSize;
            while (collectionSize < MaxCollectionSize)
            {
                _lineSeriesIndex = 0;
                foreach (var b in benchmarks)
                {
                    b.CollectionSize = collectionSize;
                    updateStatusLabel("Running " + b.BenchMarkName() + " with collection size " + collectionSize);
                    var benchmark = b.Benchmark(_maxCount, _repeats, MaxExecutionTimeInSeconds, this, _runWarmups);
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                        _plotter.AddDataPoint(_lineSeriesIndex, benchmark)));
                    Thread.Sleep(100);
                    _lineSeriesIndex++;
                    updateProgressBar(benchmarks.Length);
                }
                collectionSize *= CollectionMultiplier;
            }
            UpdateRunningLabel("");
            updateStatusLabel("Finished");
            Thread.Sleep(1000);
            updateStatusLabel("");
        }
        #endregion

        #region Util
        private void savePdf(object sender, RoutedEventArgs routedEventArgs)
        {
            var dlg = new SaveFileDialog
            {
                FileName = Benchmarks[0].BenchMarkName(),
                DefaultExt = ".pdf",
                Filter = "PDF documents (.pdf)|*.pdf"
            };

            // Show save file dialog box
            var result = dlg.ShowDialog();
            if (result != true) return;

            // Save document
            var path = dlg.FileName;
            _plotter.ExportPdf(path, ActualWidth, ActualHeight);
        }

        #endregion

        #region UI Utils
        private void updateProgressBar(int numberOfBenchmarks)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal,
                new Action(() => progress.Value += (100.0 / MaxIterations) / numberOfBenchmarks));
        }

        private void updateStatusLabel(String s)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => StatusLabel.Content = s));
        }

        public void UpdateRunningLabel(String s)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => RunningLabel.Content = s));
        }

        private void CheckBox_Checked_RunWarmups(object sender, RoutedEventArgs e)
        {
            _runWarmups = true;
        }

        private void CheckBox_Unchecked_RunWarmups(object sender, RoutedEventArgs e)
        {
            _runWarmups = false;
        }

        private void CheckBox_Checked_RunQuick(object sender, RoutedEventArgs e)
        {
            _repeats = 1;
            _maxCount = Int32.MaxValue / 1000;
        }

        private void CheckBox_Unchecked_RunQuick(object sender, RoutedEventArgs e)
        {
            _repeats = StandardRepeats;
            _maxCount = Int32.MaxValue / 10;
        }
        
        private void CheckBox_Checked_LogarithmicXAxis(object sender, RoutedEventArgs e)
        {
            _plotter.ToggleLogarithmicAxis(true);
        }

        private void CheckBox_Unchecked_LogarithmicXAxis(object sender, RoutedEventArgs e)
        {
            _plotter.ToggleLogarithmicAxis(false);
        }

        private void CheckBox_Checked_RunSequential(object sender, RoutedEventArgs e)
        {
            _runSequential = true;
        }

        private void CheckBox_Unchecked_RunSequential(object sender, RoutedEventArgs e)
        {
            _runSequential = false;
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }
        #endregion
    }
}