using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WireMock.Server;

namespace Http.Options.UnitTests
{
    [TestFixture]
    public class HttpClientContextTracingTests
    {
        private readonly WireServer _server;
        private WireMockServer _wireServer;

        public HttpClientContextTracingTests()
        {
            _wireServer = WireMockServer.Start();
            _server = new WireServer(_wireServer);
        }

        [Test]
        public async Task HttpTracing_SanityTest()
        {
            var serviceCollection = new ServiceCollection();
            HttpRequestTracingContext tracingCtx = null;
            var source = new ActivitySource("http-client-test");

            // var startTime = Stopwatch.GetTimestamp();
            serviceCollection.AddHttpClientOptions(options =>
                {
                    options.ServiceName = "service";
                    options.Handler.MaxConnection = 100;
                    // options.Tracing.Activity.Source = source;
                    options.Tracing.Tags.Config.Name = "name";
                    options.Tracing.Tags.Config.Port = "port";
                    options.Tracing.Tags.Config.Schema = "schema";
                    options.Tracing.Tags.Config.MaxConnection = "maxConnection";
                    options.Tracing.Tags.Request.Schema = "r.schema";
                    options.Tracing.Tags.Request.RequestLength = "size";
                    options.Tracing.Tags.Request.RequestPath = "path";
                    options.Tracing.Tags.Request.Host = "host";
                    // options.Tracing.TraceEnd += context => tracingCtx = context;
                    _server.ConfigureWireMockServer(options);
                })
                .AddOpenTelemetry(builder => builder
                    .AddConsoleExporter())
                ;
            //  .ProcessActivityEnd(context => tracingCtx = context);
            serviceCollection.ConfigureAll<HttpTracingOptions>(o =>
                o.OnActivityEnd(context => tracingCtx = context));
            
            var services = serviceCollection.BuildServiceProvider();
            await Task.WhenAll(services.GetServices<IHostedService>()
                .Select(e => e.StartAsync(CancellationToken.None)));
            var factory = services.GetRequiredService<IHttpClientFactory>();

            var client = factory.CreateClient("service");
            await client.GetAsync("/delay/5ms");
            var startTime = Stopwatch.GetTimestamp();

            var result = await client.GetAsync("/delay/1s");
            Assert.NotNull(tracingCtx );

            Assert.Multiple(() =>
            {
                // Assert.That(tracingCtx.RequestStartTimestamp, Is.EqualTo(startTime).Within(TimeSpan.FromMilliseconds(100).Ticks)); 
                // Assert.That(tracingCtx.ResponseEndTimestamp, Is.Not.Null.And.GreaterThan( tracingCtx.RequestStartTimestamp));

                AssertTag(tracingCtx, "time.start", Is.EqualTo(tracingCtx.RequestStartTimestamp));
                AssertTag(tracingCtx, "name", Is.EqualTo("service"));
                AssertTag(tracingCtx, "maxConnection", Is.EqualTo(100));
                AssertTag(tracingCtx, "schema", Is.EqualTo("http"));
                AssertTag(tracingCtx, "r.schema", Is.EqualTo("http"));
                AssertTag(tracingCtx, "host", Is.EqualTo("127.0.0.1"));
                AssertTag(tracingCtx, OpenTelemetryConventions.AttributeHttpMethod, Is.EqualTo("GET"));
                AssertTag(tracingCtx, "path", Is.EqualTo("/delay/1s"));
                AssertTag(tracingCtx, OpenTelemetryConventions.AttributeHttpStatusCode, Is.Not.Null.And.EqualTo(200));
                AssertTag(tracingCtx, "time.end", Is.Not.Null.And.EqualTo(tracingCtx.ResponseEndTimestamp));
            });
        }


        [Test]
        public async Task HttpTracing_OnFailure()
        {
            var serviceCollection = new ServiceCollection();
            HttpRequestTracingContext tracingCtx = null;
            var source = new ActivitySource("http-client-test");

            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "service";
                options.Handler.MaxConnection = 100;
                options.Tracing.Tags.Config.Name = "name";
                options.Tracing.Tags.Config.Port = "port";
                options.Tracing.Tags.Config.Schema = "schema";
                options.Tracing.Tags.Config.MaxConnection = "maxConnection";
                options.Tracing.Tags.Request.Schema = "r.schema";
                options.Tracing.Tags.Request.RequestLength = "size";
                options.Tracing.Tags.Request.RequestPath = "path";
                options.Tracing.Tags.Request.Host = "host";
                options.Tracing.TraceEnd += context => tracingCtx = context;
                _server.ConfigureWireMockServer(options);
            }).AddOpenTelemetry(builder => builder
                .AddConsoleExporter());
            ;
            serviceCollection.ConfigureAll<HttpTracingOptions>(o =>
                o.OnActivityEnd(context => tracingCtx = context));

            var services = serviceCollection.BuildServiceProvider();
            await Task.WhenAll(services.GetServices<IHostedService>()
                .Select(e => e.StartAsync(CancellationToken.None)));

            var factory = services.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient("service");

            //worm up
            await client.GetAsync("/delay/5ms");
            var startTime = Stopwatch.GetTimestamp();

            var result = await client.GetAsync("/error/5ms");
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.NotNull(tracingCtx );

            Assert.Multiple(() =>
            {
                // Assert.That(tracingCtx.RequestStartTimestamp, Is.EqualTo(startTime).Within(TimeSpan.FromMilliseconds(100).Ticks)); 
                // Assert.That(tracingCtx.ResponseEndTimestamp, Is.Not.Null.And.GreaterThan( tracingCtx.RequestStartTimestamp));

                AssertTag(tracingCtx, "time.start", Is.EqualTo(tracingCtx.RequestStartTimestamp));
                AssertTag(tracingCtx, "name", Is.EqualTo("service"));
                AssertTag(tracingCtx, "maxConnection", Is.EqualTo(100));
                AssertTag(tracingCtx, "schema", Is.EqualTo("http"));
                AssertTag(tracingCtx, "r.schema", Is.EqualTo("http"));
                AssertTag(tracingCtx, "host", Is.EqualTo("127.0.0.1"));
                AssertTag(tracingCtx, OpenTelemetryConventions.AttributeHttpUrl, Is.EqualTo(_server.Url("error/5ms")));
                AssertTag(tracingCtx, "path", Is.EqualTo("/error/5ms"));
                AssertTag(tracingCtx, OpenTelemetryConventions.AttributeHttpStatusCode, Is.Not.Null.And.EqualTo(500));
                AssertTag(tracingCtx, "time.end", Is.Not.Null.And.EqualTo(tracingCtx.ResponseEndTimestamp));
            });
        }


        [Test]
        public async Task HttpTracing_OnTimeout()
        {
            var serviceCollection = new ServiceCollection();
            HttpRequestTracingContext tracingCtx = null;

            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "service";
                options.Timeout.TimeoutMS = 5;
                options.Handler.MaxConnection = 100;
                options.Tracing.Tags.Config.Name = "name";
                options.Tracing.Tags.Config.Port = "port";
                options.Tracing.Tags.Config.Schema = "schema";
                options.Tracing.Tags.Config.MaxConnection = "maxConnection";
                options.Tracing.Tags.Request.Schema = "r.schema";
                options.Tracing.Tags.Request.RequestLength = "size";
                options.Tracing.Tags.Request.RequestPath = "path";
                options.Tracing.Tags.Request.Host = "host";
                options.Tracing.TraceEnd += context => tracingCtx = context;
                _server.ConfigureWireMockServer(options);
            }).AddOpenTelemetry(builder => builder
                .AddConsoleExporter());
            ;
            serviceCollection.ConfigureAll<HttpTracingOptions>(o =>
                o.OnActivityEnd(context => tracingCtx = context));

            var services = serviceCollection.BuildServiceProvider();
            await Task.WhenAll(services.GetServices<IHostedService>()
                .Select(e => e.StartAsync(CancellationToken.None)));

            var factory = services.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient("service");

            // //worm up
            // await client.GetAsync("/delay/5ms");
            var startTime = Stopwatch.GetTimestamp();

            Assert.ThrowsAsync<TimeoutException>(async () => await client.GetAsync("/delay/200ms"));
            Assert.NotNull(tracingCtx);

            Assert.Multiple(() =>
            {
                // Assert.That(tracingCtx.RequestStartTimestamp,
                //     Is.EqualTo(startTime).Within(TimeSpan.FromMilliseconds(100).Ticks));
                // Assert.That(tracingCtx.ResponseEndTimestamp,
                //     Is.Not.Null.And.GreaterThan(tracingCtx.RequestStartTimestamp));

                AssertTag(tracingCtx, "time.start", Is.EqualTo(tracingCtx.RequestStartTimestamp));
                AssertTag(tracingCtx, "name", Is.EqualTo("service"));
                AssertTag(tracingCtx, "maxConnection", Is.EqualTo(100));
                AssertTag(tracingCtx, "config.timeout", Is.EqualTo(5));
                AssertTag(tracingCtx, "schema", Is.EqualTo("http"));
                AssertTag(tracingCtx, "r.schema", Is.EqualTo("http"));
                AssertTag(tracingCtx, "host", Is.EqualTo("127.0.0.1"));
                AssertTag(tracingCtx, "path", Is.EqualTo("/delay/200ms"));
                AssertTag(tracingCtx, OpenTelemetryConventions.AttributeHttpStatusCode, Is.Null);
                AssertTag(tracingCtx, "time.end", Is.Not.Null.And.EqualTo(tracingCtx.ResponseEndTimestamp));
                AssertTag(tracingCtx, "time.total", Is.Not.Null.And.EqualTo(tracingCtx.TotalTime));
            });
        }

        private static void AssertTag(HttpRequestTracingContext context, string name, IConstraint constraint)
        {
            context.Tags.TryGetValue(name, out var tag);
            Assert.That(tag, constraint, name);
        }
    }
}