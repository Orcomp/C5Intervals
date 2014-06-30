using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using C5.Intervals;

namespace C5.UserGuideExamples.Intervals
{
    public class FeedReader
    {
        /// <summary>
        /// Parses an icalendar (ics) of concerts from the Copenhagen Jazz Festival 2009 and prints some statistics about the festival utilizing an interval data structure.
        /// </summary>
        public static void CopenhagenJazz()
        {
            // TODO: http://www.kanzaki.com/docs/ical/
            const string url = "http://www.gofish.dk/cjf2009.ics"; // Copenhagen Jazz Festival 2009 Concerts

            // Parse url
            var events = ParseUrl(url, FeedType.Ics);

            // Create interval collection
            var coll = new DynamicIntervalTree<CalendarEvent, DateTime>(events);

            // Let us play with the data we have from the Copenhagen Jazzfestival 2009 ical feed.
            // Startdate of the festival
            var start = coll.Span.Low;
            // Instead of using the spans low as start, we build a "clean" date that starts at midnight
            var startDate = new DateTime(year: start.Year, month: start.Month, day: start.Day);
            var end = coll.Span.High;
            // We also build a cleaner end date which ends at midnight on the last day
            var endDate = new DateTime(year: end.Year, month: end.Month, day: end.Day);

            // How many days is the festival running
            var numberOfdays = endDate.Subtract(startDate).Days + 1;

            // Create an array with events spanning each day of the festival
            var dates = new CalendarEvent[numberOfdays];
            for (var day = 0; day < numberOfdays; day++)
                dates[day] = new CalendarEvent("Festival day " + day, startDate.AddDays(day), startDate.AddDays(day + 1));

            // Print some general statistics
            Console.Out.WriteLine("Copenhagen Jazz Festival 2009 statistics");
            Console.Out.WriteLine("Total number of concerts {0}.", coll.Count());
            Console.Out.WriteLine("The maximum number of simultaneous concerts is {0}.", coll.MaximumDepth);

            // Print statistics for each day of the festival
            foreach (var day in dates.Select(day => new DynamicIntervalTree<CalendarEvent, DateTime>(coll.FindOverlaps(day))))
                Console.Out.WriteLine("There are {0,-3} concerts on the {1}. {2, -2} of these are simultaneous.", day.Count, day.Choose().Low.ToShortDateString(), day.MaximumDepth);
            Console.ReadLine();
        }

        public class CalendarEvent : IInterval<DateTime>, IComparable<CalendarEvent>
        {
            public string Title { get; private set; }
            public DateTime Low { get; private set; }
            public DateTime High { get; private set; }
            public bool LowIncluded { get { return true; } }
            public bool HighIncluded { get { return false; } }
            public TimeSpan Duration { get { return High.Subtract(Low); } }

            public CalendarEvent(string title, DateTime startTime, DateTime endTime)
            {
                Title = title;
                Low = startTime;
                High = endTime;
            }

            public int CompareTo(CalendarEvent other)
            {
                return IntervalExtensions.CompareTo(this, other);
            }

            public override string ToString()
            {
                return String.Format("{0,-15}: {1} (duration: {2})", Title, this.ToIntervalString(), Duration);
            }
        }

        public enum FeedType
        {
            Ics
        }

        public static IEnumerable<CalendarEvent> ParseUrl(string url, FeedType type)
        {
            // In case of IOException take a look here: http://stackoverflow.com/questions/14432079/wcf-the-specified-registry-key-does-not-exist-in-base-channel-call#14432540
            return ParseString(new WebClient { Encoding = System.Text.Encoding.UTF8 }.DownloadString(url), type);

        }

        public static IEnumerable<CalendarEvent> ParseString(string feed, FeedType type)
        {
            // All the parsed events
            var events = new ArrayList<CalendarEvent>();

            // Example datestring to get the correct length for parsing
            var dateLength = @"19700101T000000Z".Length;

            int begin, end = 0;

            while ((begin = feed.IndexOf("BEGIN:VEVENT", end)) >= 0)
            {
                // Find end of event
                end = feed.IndexOf("END:VEVENT", begin);

                // Take a substring of the vevent found
                var vevent = feed.Substring(begin, end - begin);

                // Find title
                var index = vevent.IndexOf("SUMMARY");
                index = vevent.IndexOf(":", index) + 1;
                var titlePos = vevent.IndexOf("\r", index);
                if (titlePos < 0)
                    titlePos = vevent.IndexOf("\n", index);
                var title = vevent.Substring(index, titlePos - index);

                // Find start time
                index = vevent.IndexOf("DTSTART");
                index = vevent.IndexOf(":", index) + 1;
                var low = stringToDate(vevent.Substring(index, dateLength));

                // Find end time
                index = vevent.IndexOf("DTEND");
                index = vevent.IndexOf(":", index) + 1;
                var high = stringToDate(vevent.Substring(index, dateLength));

                // Create and add calendar event to list
                events.Add(new CalendarEvent(title, low, high));
            }

            // Sort events
            var eventArray = events.ToArray();
            Sorting.IntroSort(eventArray);

            return eventArray;
        }

        private static DateTime stringToDate(String dateString)
        {
            // TODO: Fix to parse the ICS date/date-time format
            var correctedDateString = dateString.Substring(0, dateString.Length - 3).Insert(4, "-").Insert(7, "-").Insert(13, ":");

            // Adding two hours here to match the Copenhagen Jazzfestival Feed to GMT+1 (Copenhagen Timezone).
            return DateTime.Parse(correctedDateString).AddHours(2);
        }
    }
}
