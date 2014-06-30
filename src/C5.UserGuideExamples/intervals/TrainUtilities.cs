using System;
using System.Collections.Generic;
using System.Linq;
using C5.Intervals;

namespace C5.UserGuideExamples.Intervals
{
    // Helper class used by some of the train benchmarks
    public class TrainUtilities
    {
        public const int TrainSetACount = 21998;
        public const int TrainSetBCount = 31156;

        /// <summary>
        /// This method decides which train data gets used when running the train benchmarks.
        /// </summary>
        /// <param name="numberOfTrainsToGet">Decides how many trains to get from the source data.</param>
        /// <returns></returns>
        public static Trains.TrainRide[] GetTrains(int numberOfTrainsToGet)
        {
            // Return the trains with ressources - number of trains 21998
            return Trains.ParseTrainSetA().SelectMany(col => col).Take(numberOfTrainsToGet).ToArray();

            // Return the trains without ressources - number of trains 31156
            return Trains.ParseTrainSetB().Take(numberOfTrainsToGet).ToArray();
        }

        public static IEnumerable<InBetweenTrainRide> FindInbetweenTrains(IEnumerable<Trains.TrainRide> resources)
        {
            var sortedTrains = resources.ToList();
            sortedTrains.Sort((t1, t2) => t1.CompareTo(t2));
            var intervalsNotInCollection = new ArrayList<InBetweenTrainRide>();
            var highestHigh = 0.0;
            for (var i = 0; i < sortedTrains.Count - 1; i++)
            {
                var i1 = sortedTrains[i];
                var i2 = sortedTrains[i + 1];
                if (i1.High.CompareTo(highestHigh) > 0)
                    highestHigh = i1.High;
                if (i1.High.CompareTo(i2.Low) < 0 && i2.Low.CompareTo(highestHigh) >= 0)
                    intervalsNotInCollection.Add(new InBetweenTrainRide(i1.High, i2.Low, i1.Track, i1.Train));
                if (i1.High.CompareTo(i2.Low) < 0 && i2.Low.CompareTo(highestHigh) >= 0 && i == sortedTrains.Count - 2)
                    intervalsNotInCollection.Add(new InBetweenTrainRide(i1.High, i2.Low, i1.Track, i1.Train));
            }
            //            Console.Out.WriteLine("There is {0} intervals in the collection.", sortedTrains.Count);
            //            Console.Out.WriteLine("Created {0} intervals that's not in the collection.", intervalsNotInCollection.Count);

            // Check that we have made intervals that truly don't exist in the collections
            foreach (var interval in intervalsNotInCollection.Where(sortedTrains.Contains))
                Console.Out.WriteLine(
                    "You have made a terrible mistake, this interval {0} should not be in the collection!", interval);
            return intervalsNotInCollection;
        }

        public class InBetweenTrainRide : Trains.TrainRide
        {
            public InBetweenTrainRide(double low, double high, int track, int train)
                : base(low, high, track, train)
            {
            }

            public override string ToString()
            {
                return this.ToIntervalString();
            }
        }

        public static void PrintTrainStatistics(ArrayList<Trains.TrainRide> resources)
        {
            var numberOfTrains = resources.Select(tr => tr.Train).ToArray();
            Console.Out.WriteLine("Number of trains: " + numberOfTrains.Count() + "\nDistinct number of trains: " + numberOfTrains.Distinct().Count());

            var numberOfTracks = resources.Select(tr => tr.Track).Distinct().ToList();
            numberOfTracks.Sort();
            Console.Out.WriteLine("Distinct number of tracks: " + numberOfTracks.Count());

            var trainsOnSeperateTracks = new List<Trains.TrainRide>[numberOfTracks.Count];
            for (var i = 0; i < numberOfTracks.Count; i++)
                trainsOnSeperateTracks[i] =
                    resources.Filter(tr => tr.Track == numberOfTracks[i]).ToList();

            for (var i = 0; i < numberOfTracks.Count; i++)
            {
                var track = numberOfTracks[i];
                var trains = resources.Filter(tr => tr.Track == track).ToArray();
                var trainIds = trains.Select(tr => tr.Train).Distinct();
                var distinctTrains = trainIds.Select(trainId => trains.First(tr => tr.Train == trainId));
                Console.Out.WriteLine("Track " + track + " is used by " + distinctTrains.Count() + " train(s).");

                foreach (var trainRide in trains)
                {
                    var collision =
                        trainsOnSeperateTracks[i].FindAll(
                        // Detect possible collisions
                                tr => tr.Train != trainRide.Train &&
                                      tr.Track == trainRide.Track &&
                                      tr.Overlaps(trainRide)
                                );
                    if (collision.Any())
                        Console.Out.WriteLine("{0} collision(s) detected on track {1}, between train {2} & {3}.",
                            collision.Count(), trainRide.Track,
                            trainRide.Train,
                            String.Join(",", collision.Select(tr => tr.Train).ToArray()));
                }
            }
        }
    }
}
