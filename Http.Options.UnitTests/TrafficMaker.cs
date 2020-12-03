using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Http.Options.UnitTests
{
    public static class TrafficMaker
    {
        public static IObservable<T> GenerateTraffic<T>(int rps, Func<Task<T>> action)
        {
            return Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Select(x => Enumerable
                    .Range(0, rps)
                    .Select(x => action())
                )
                .SelectMany(x => x)
                .SelectMany(x => x);
        }


        public static IObservable<(double median, double avarage, double max, double min)> Rps<T>(
            this IObservable<T> requests)
        {
            return requests
                .Buffer(TimeSpan.FromSeconds(1))
                .Select(x => (double) x.Count)
                .Statistics();
        }

        public static IObservable<(double median, double avarage, double max, double min)> Latency<T>(
            this IObservable<T> requests)
        {
            return requests
                .TimeInterval()
                .Select(x => (double) x.Interval.Milliseconds)
                .Statistics();
        }

        public static IObservable<(double median, double avarage, double max, double min)> Statistics(
            this IObservable<double> data)

        {
            return data.Scan(new List<double>(), (list, element) =>
                {
                    list.Add(element);
                    return list;
                })
                .Select(x =>
                (
                    median: x
                        .OrderBy(y => y)
                        .Skip(x.Count / 2)
                        .First(),
                    avarage: x
                        .Average(),
                    max: x
                        .Max(),
                    min: x.Min()));
        }
    }
}