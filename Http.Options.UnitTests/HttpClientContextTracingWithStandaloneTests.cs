using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Http.Client.Options.Tracing;
using Http.Options.Counters;
using Http.Options.Standalone;
using Http.Options.Tracing.OpenTelemetry;
using Http.Options.Tracing.OptionsBuilder;
using Http.Options.Tracing.Processors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WireMock.Server;

namespace Http.Options.UnitTests
{
    [TestFixture, Parallelizable(ParallelScope.None)]
    public class HttpClientStandaloneTracingTests
    {
        private readonly WireServer _server;
        private readonly WireMockServer _wireServer;
        private IServiceProvider _services;

        private readonly Dictionary<string, HttpTracingActivity> _activityMap =
            new Dictionary<string, HttpTracingActivity>();

        private readonly List<HttpTracingActivity> _activities = new List<HttpTracingActivity>();
        private IHttpClientFactory _factory;
        private readonly ActivitySource activitySource = new ActivitySource("my-source");

        public HttpClientStandaloneTracingTests()
        {
            _wireServer = WireMockServer.Start();
            _server = new WireServer(_wireServer);
        }

        private async Task<IHttpClientFactory> BuildServices()
        {
            var builder = HttpOptionsBuilder.Configure(delegate(HttpOptionsBuilder builder)
                {
                    builder.Services.AddCountersTracing();
                    builder.Services.AddHttpOptionsTelemetry(
                        delegate(OpenTelemetryOptionsBuilder openTelemetryOptionsBuilder)
                        {
                            openTelemetryOptionsBuilder.ConfigureExportAction(activity => _activities.Add(activity));

                            openTelemetryOptionsBuilder.ConfigureOpenTelemetryBuilder(providerBuilder =>
                            {
                                providerBuilder.AddConsoleExporter();
                            });
                            openTelemetryOptionsBuilder.ConfigureTracing(options =>
                            {
                                options.Activity.Source = activitySource;
                            });

                            openTelemetryOptionsBuilder.ConfigureTags(options =>
                            {
                                options.Config.Name = "name";
                                options.Config.Port = "port";
                                options.Config.Schema = "schema";
                                options.Config.MaxConnection = "maxConnection";
                                options.Request.Schema = "r.schema";
                                options.Request.RequestLength = "size";
                                options.Request.RequestPath = "path";
                                options.Request.Host = "host";
                            });
                        });
                    builder.Configure(collectionOptions =>
                    {
                        collectionOptions.Defaults.Connection.Server = "defaults.com";

                        collectionOptions.AddClient("setup", options =>
                        {
                            options.Handler.MaxConnection = 100;
                            _server.ConfigureWireMockServer(options);
                        });


                        collectionOptions.AddClient("service", options =>
                        {
                            options.Handler.MaxConnection = 100;
                            _server.ConfigureWireMockServer(options);
                        });

                        collectionOptions.AddClient("service_failure", options =>
                        {
                            options.Handler.MaxConnection = 100;
                            _server.ConfigureWireMockServer(options);
                        });

                        collectionOptions.AddClient("service_timeout", options =>
                        {
                            options.Timeout.TimeoutMS = 5;
                            options.Handler.MaxConnection = 100;
                            _server.ConfigureWireMockServer(options);
                        });
                    });
                })
                .Build();
            await builder.StartAsync(CancellationToken.None);
            return builder;
        }

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            _factory = await BuildServices();
        }


        [Test, Order(2)]
        public async Task HttpTracing_SanityTest()
        {
            var serviceName = "service";
            var client = _factory.CreateClient(serviceName);

            var timing = await Time(() => client.GetAsync("/delay/200ms"));

            HttpTracingActivity tracingCtx = LastActivity(serviceName);

            Assert.NotNull(tracingCtx);
            Assert.Multiple(() =>
            {
                timing.AssertTime(tracingCtx);
                AssertConfig(tracingCtx, serviceName);
                AssertRequest(tracingCtx, "/delay/200ms");
                AssertResponse(tracingCtx, 200);
                AssertConnection(tracingCtx);
            });
        }


        [Test, Order(1)]
        public async Task HttpTracing_OnFailure()
        {
            var serviceName = "service_failure";

            var client = _factory.CreateClient(serviceName);
            var timing = await Time(() => client.GetAsync("/error/5ms"));

            var tracingCtx = LastActivity(serviceName);
            Assert.NotNull(tracingCtx);
            Assert.Multiple(() =>
            {
                // timing.AssertTime(tracingCtx); 
                AssertConfig(tracingCtx, serviceName);
                AssertRequest(tracingCtx, "/error/5ms");
                AssertResponse(tracingCtx, 500);
                AssertConnection(tracingCtx);
            });
        }

        private void AssertConnection(HttpTracingActivity httpActivity)
        {
#if NETFRAMEWORK
            AssertTag(httpActivity, "connection.count", Is.GreaterThanOrEqualTo(1));
            AssertTag(httpActivity, "connection.limit", Is.GreaterThanOrEqualTo(100));
            AssertTag(httpActivity, "connection.timeout", Is.GreaterThanOrEqualTo(-1));
            AssertTag(httpActivity, "connection.idleSince", Is.Not.Null);
            AssertTag(httpActivity, "connection.maxIdleTime", Is.GreaterThanOrEqualTo(100000));
            AssertTag(httpActivity, "connection.useNagle", Is.True);

#endif
        }


        [Test, Order(3)]
        public async Task HttpTracing_OnTimeout()
        {
            var serviceName = "service_timeout";

            var client = _factory.CreateClient(serviceName);
            var timing = await Time(() => client.GetAsync("/delay/200ms"));

            var tracingCtx = LastActivity(serviceName);
            Assert.NotNull(tracingCtx);
            Assert.Multiple(() =>
            {
                // timing.AssertTime(tracingCtx);
                AssertConfig(tracingCtx, serviceName);
                AssertRequest(tracingCtx, "/delay/200ms");
                AssertConnection(tracingCtx);

                AssertTag(tracingCtx, OpenTelemetryConventions.AttributeHttpStatusCode, Is.Null);
                AssertTag(tracingCtx, "config.timeout", Is.EqualTo(5));
            });
        }

        private static void AssertConfig(HttpTracingActivity httpActivity, string serviceName)
        {
            AssertTag(httpActivity, "name", Is.EqualTo(serviceName));

            AssertTag(httpActivity, "maxConnection", Is.EqualTo(100));
            AssertTag(httpActivity, "schema", Is.EqualTo("http"));
        }

        private void AssertRequest(HttpTracingActivity httpActivity, string path)
        {
            AssertTag(httpActivity, "r.schema", Is.EqualTo("http"));
            AssertTag(httpActivity, "host", Is.EqualTo("127.0.0.1"));
            AssertTag(httpActivity, "path", Is.EqualTo(path));
            AssertTag(httpActivity, OpenTelemetryConventions.AttributeHttpMethod, Is.EqualTo("GET"));
            AssertTag(httpActivity, OpenTelemetryConventions.AttributeHttpUrl, Is.EqualTo(_server.Url(path)));
        }

        private static void AssertResponse(HttpTracingActivity httpActivity, int statusCode)
        {
            AssertTag(httpActivity, OpenTelemetryConventions.AttributeHttpStatusCode, Is.EqualTo(statusCode));
        }

        private static void AssertTag(HttpTracingActivity activity, string name, IConstraint constraint)
        {
            activity.Tags.TryGetValue(name, out var tag);
            Assert.That(tag, constraint, name);
        }

        private HttpTracingActivity LastActivity(string service)
        {
            return _activities.LastOrDefault(x => x.ClientOptions.ServiceName == service);
        }

        private static async Task<AsyncTiming> Time(Func<Task> task)
        {
            var timing = new AsyncTiming(task);
            await timing.Await;
            return timing;
        }


        private class AsyncTiming
        {
            public readonly long Timestamp;
            public readonly DateTime StartDate;
            public readonly Stopwatch Stopwatch;
            public DateTime EndTime;
            public readonly Task Await;

            public AsyncTiming(Func<Task> taskFactory)
            {
                Timestamp = Stopwatch.GetTimestamp();
                StartDate = DateTime.UtcNow;
                Stopwatch = Stopwatch.StartNew();
                Await = taskFactory().ContinueWith(x =>
                {
                    Stopwatch.Stop();
                    EndTime = DateTime.UtcNow;
                });
            }


            public void AssertTime(HttpTracingActivity httpActivity)
            {
                // Assert.That(httpActivity.Timestamp, Is.EqualTo(Timestamp).Within(20000),
                //     $"timestamp differ by {Timestamp - httpActivity.Timestamp}");
                Assert.That(httpActivity.StartTime, Is.EqualTo(StartDate).Within(20).Milliseconds, "startTime");
                Assert.That(httpActivity.EndTime, Is.EqualTo(EndTime).Within(20).Milliseconds, "endTime");
                Assert.That(httpActivity.TotalTime, Is.EqualTo(Stopwatch.Elapsed).Within(300).Milliseconds,
                    "totalTime");


                AssertTag(httpActivity, "timestamp", Is.EqualTo(httpActivity.Timestamp));
                AssertTag(httpActivity, "time.start", Is.EqualTo(httpActivity.Activity.StartTimeUtc));
                AssertTag(httpActivity, "time.end",
                    Is.EqualTo(httpActivity.Activity.StartTimeUtc.Add(httpActivity.Activity.Duration)));
                AssertTag(httpActivity, "time.duration", Is.EqualTo(httpActivity.Activity.Duration));

                AssertTag(httpActivity, "time.http.start", Is.EqualTo(httpActivity.HttpActivity.StartTimeUtc));
                AssertTag(httpActivity, "time.http.end",
                    Is.EqualTo(httpActivity.HttpActivity.StartTimeUtc.Add(httpActivity.HttpActivity.Duration)));
                AssertTag(httpActivity, "time.http.duration", Is.EqualTo(httpActivity.HttpActivity.Duration));
            }
        }
    }
}