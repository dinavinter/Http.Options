using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using  FluTeLib.Core.helper.Linq;

namespace Http.Options.UnitTests
{
     public class Result<T>
    {
        public T Value;
        public bool IsError => Error != null;
        public Exception Error;
    }

    public class Stats
    {
        public double Median;
        public double Avarage;
        public double Max;
        public double Min;
        public double Total;

        public string Print()
        {
            return $"median:{Median} | avg:{Avarage} | max:{Max} | min:{Min}";
        }
    }

    public class TrafficStats
    {
        public Stats Success;
        public Stats Errors;
        public Exception LastError;
        public double ErrorRate => (Success.Total + Errors.Total) / Errors.Total;

        public string Print()
        {
            return $"success rate: {Success.Print()}\r\nerror rate: {Errors.Print()}";
        }
    }

    public static class TrafficGenerator
    {
        private static readonly string[] _ids = Enumerable
            .Range(0, 10000)
            .Select(x => GetString())
            .ToArray();

        public static Stack<string> IdsPool(int rate, int runFor, int diversion)
        {
            if (diversion > _ids.Length)
                return new Stack<string>(_ids);

            if (diversion > (runFor * rate))
                return new Stack<string>(_ids.Take(diversion));

            return new Stack<string>(Enumerable
                .Repeat(_ids.Take(diversion), (runFor * rate * 2) / diversion)
                .SelectMany(x => x)
                .Shuffle());
        }

        public static IObservable<Result<T>> GenerateTraffic<T>(int rps, Func<Task<T>> action)
        {
            return Observable
                .Interval(TimeSpan.FromSeconds(1))
                .SelectMany(_ => Observable
                    .Range(0, rps)
                    .Select(_ => Try())
                    .SelectMany(x => x));


            async Task<Result<T>> Try()
            {
                try
                {
                    return new Result<T>()
                    {
                        Value = await action()
                    };
                }
                catch (Exception e)
                {
                    await Console.Error.WriteAsync(e.Message);
                    await Task.Delay(5);
                    return new Result<T>()
                    { 
                        Error= e
                    };
                }
            }
        }

        public static IObservable<(int success, int errors, Exception lastError)> RPS<T>(
            this IObservable<Result<T>> requests)
        {
            return requests
                .Buffer(TimeSpan.FromSeconds(1))
                .Select(x => (x.Count(e => !e.IsError), x.Count(e => e.IsError), x.LastOrDefault(e => e.IsError)?.Error));
        }

        public static IObservable<TrafficStats> Stats(
            this IObservable<(int success, int errors, Exception lastError)> rps, double stopOnErrorRate= 0.3)
        {
            var stats= rps
                .Scan(new List<(int success, int errors, Exception lastError)>(), (list, element) =>
                {
                    list.Add(element);
                    return list;
                })
                .Select(x =>
                    new TrafficStats()
                    {
                        Errors = x.Select(e => e.errors).Stats(),
                        Success = x.Select(e => e.success).Stats(),
                        LastError = x.LastOrDefault(e => e.lastError != null).lastError
                    }
                );

            stats.Where(e => e.ErrorRate > 0.3).Do(x => Assert.Fail( x.LastError.Message));

            return stats;

        }
        
       
        public static Stats Stats(this IEnumerable<int> data)
        {
            var enumerable = data as int[] ?? data.ToArray();
            return  
                new Stats()
                {
                    Median = enumerable
                        .OrderBy(y => y)
                        .Skip(enumerable.Count() / 2)
                        .First(),

                    Avarage = enumerable.Average(),
                    Max = enumerable.Max(),
                    Min = enumerable.Min(),
                    Total = enumerable.Sum()
                };
        }

        public static IObservable<Stats> Stats(
            this IObservable<double> data)

        {
            return data.Scan(new List<double>(), (list, element) =>
                {
                    list.Add(element);
                    return list;
                })
                .Select(x =>
                    new Stats()
                    {
                        Median = x
                            .OrderBy(y => y)
                            .Skip(x.Count / 2)
                            .First(),

                        Avarage = x.Average(),
                        Max = x.Max(),
                        Min = x.Min(),
                        Total = x.Sum()
                    });
        }

     

        public static IObservable<Stats> Latency<T>(
            this IObservable<T> requests)
        {
            return requests
                .TimeInterval()
                .Select(x => (double) x.Interval.Milliseconds)
                .Stats();
        }
        
        public const string AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        [ThreadStatic] private static Random _randomField = new Random(Guid.NewGuid().GetHashCode());

        private static Random _random
        {
            get { return _randomField = _randomField ?? new Random(Guid.NewGuid().GetHashCode()); }
        }


        public static string GetString(int length = 12)
        {
            return new string(Enumerable.Repeat(AllowedChars, length).Select(s => s[_random.Next(s.Length)]).ToArray());
        }


    
    }

}