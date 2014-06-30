using System;
using System.Globalization;
using System.Linq;
using C5.Intervals;
using Microsoft.VisualBasic.FileIO;

namespace C5.UserGuideExamples.Intervals
{
    public class Trains
    {
        public static void Main(string[] args)
        {
            Console.Out.WriteLine(train());
            Console.Out.WriteLine("Maximum depth of Set A " + new DynamicIntervalTree<TrainRide, Double>(ParseTrainSetA().SelectMany(c => c).ToArray()).MaximumDepth);
            Console.Out.WriteLine("Maximum depth of Set B " + new DynamicIntervalTree<TrainRide, Double>(ParseTrainSetB().ToArray()).MaximumDepth);

            var trains = ParseTrainSetA().SelectMany(c => c).ToArray();
            Console.Out.WriteLine("Found {0} trains in the data file.", trains.Count());
            var inbetweenTrains = TrainUtilities.FindInbetweenTrains(trains);
            Console.Out.WriteLine("Found {0} trains to put inbetween the existing trains.", inbetweenTrains.Count());
            Console.Out.WriteLine("Press any key to check for collisions...");
            Console.ReadLine();
            var trainz = new ArrayList<TrainRide>();
            trainz.AddAll(ParseTrainSetA().SelectMany(c => c).ToArray());
            TrainUtilities.PrintTrainStatistics(trainz);
            Console.Out.WriteLine("Press any key to exit.");
            Console.Read();
        }

        public static ArrayList<TrainRide>[] ParseTrainSetA()
        {
            const string filepath = @"../../../C5.UserGuideExamples/intervals/data/train.csv";

            var resources = new ArrayList<TrainRide>[16];

            using (var parser = new TextFieldParser(filepath) { Delimiters = new[] { "," } })
            {
                // Skip first line with header
                var parts = parser.ReadFields();
                while ((parts = parser.ReadFields()) != null)
                {
                    var resource = Int32.Parse(parts[0]) - 1;

                    var start = double.Parse(parts[1], CultureInfo.InvariantCulture);
                    var end = double.Parse(parts[2], CultureInfo.InvariantCulture);
                    var train = Int32.Parse(parts[3]);
                    var track = Int32.Parse(parts[4]);

                    var ride = new TrainRide(start, end, track, train);

                    if (resources[resource] == null)
                    {
                        /*
                            resources[resource] = new IntervalBinarySearchTree<TrainRide, double>();
                        /*/
                        resources[resource] = new ArrayList<TrainRide>();
                        //*/
                    }
                    resources[resource].Add(ride);
                }
            }

            return resources;
        }

        public static ArrayList<TrainRide> ParseTrainSetB()
        {
            const string filepath = @"../../../C5.UserGuideExamples/intervals/data/trainsWithoutRessources.csv";

            var resources = new ArrayList<TrainRide>();

            using (var parser = new TextFieldParser(filepath) { Delimiters = new[] { "," } })
            {
                // Skip first line with header
                var parts = parser.ReadFields();
                while ((parts = parser.ReadFields()) != null)
                {
                    var start = double.Parse(parts[0], CultureInfo.InvariantCulture);
                    var end = double.Parse(parts[1], CultureInfo.InvariantCulture);
                    var track = Int32.Parse(parts[2]);
                    var train = Int32.Parse(parts[3]);

                    var ride = new TrainRide(start, end, track, train);
                    resources.Add(ride);
                }
            }
            return resources;
        }

        public class TrainRide : IInterval<double>
        {
            public TrainRide(double low, double high, int track, int train)
            {
                High = high;
                Low = low;
                Track = track;
                Train = train;
            }

            public double Low { get; private set; }
            public double High { get; private set; }
            public bool LowIncluded { get { return true; } }
            public bool HighIncluded { get { return true; } }

            public int Track { get; private set; }
            public int Train { get; private set; }

            public override int GetHashCode()
            {
                var hash = 17;
                hash = hash * 23 + High.GetHashCode();
                hash = hash * 23 + Low.GetHashCode();
                hash = hash * 23 + Track.GetHashCode();
                hash = hash * 23 + Train.GetHashCode();
                return hash;
            }

            public override string ToString()
            {
                return this.ToIntervalString();
            }
        }



        private static string train()
        {
            return @"
                o    .  o  .  o .  o  .  o  .  o
           o
        .
      .        ___
     _n_n_n____i_i ________ ______________ _++++++++++++++_
  *>(____________I I______I I____________I I______________I
    /ooOOOO OOOOoo  oo oooo oo          oo ooo          ooo
------------------------------------------------------------";
        }

    }


}
