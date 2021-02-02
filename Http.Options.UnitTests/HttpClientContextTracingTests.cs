using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using WireMock.Server;

namespace Http.Options.UnitTests
{
    public class HttpClientContextTracingTests
    {
        private readonly WireServer _server;

        public HttpClientContextTracingTests()
        {
            _server = new WireServer(WireMockServer.Start());

        }
        
        [Test]
        public async Task SanityTest()
        {
            var serviceCollection = new ServiceCollection();
            HttpRequestTracingContext tracingCtx= null;

            var startTime = Stopwatch.GetTimestamp();
            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "service";
                options.Handler.MaxConnection = 100;
                options.Tracing.Default.Config.Name = "name";
                options.Tracing.Default.Config.Port = "port";
                options.Tracing.Default.Config.Schema = "schema";
                options.Tracing.Default.Config.MaxConnection = "maxConnection";
                options.Tracing.Default.Request.Schema = "r.schema";
                options.Tracing.Default.Request.RequestLength = "size";
                options.Tracing.Default.Request.RequestPath = "path";
                options.Tracing.Default.Request.Host = "host";
                options.Tracing.TraceEnd += context => tracingCtx = context;
                _server.ConfigureWireMockServer(options);
            });
            
            var factory = serviceCollection.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient("service");

            var result = await client.GetAsync("/delay/1s");

            Assert.Multiple(() =>
            {
                Assert.That(tracingCtx.RequestStartTimestamp, Is.EqualTo(startTime).Within(TimeSpan.FromMilliseconds(100).Ticks)); 
                Assert.That(tracingCtx.ResponseEndTimestamp, Is.Not.Null.And.GreaterThan( tracingCtx.RequestStartTimestamp));

                AssertTag(tracingCtx, "request.timestamp", Is.EqualTo(tracingCtx.RequestStartTimestamp));
                AssertTag(tracingCtx, "name", Is.EqualTo("service"));
                AssertTag(tracingCtx, "maxConnection", Is.EqualTo(100));
                AssertTag(tracingCtx, "schema", Is.EqualTo("http"));
                AssertTag(tracingCtx, "r.schema", Is.EqualTo("http"));
                AssertTag(tracingCtx, "host", Is.EqualTo("127.0.0.1"));
                AssertTag(tracingCtx, "request.query", Is.EqualTo(""));
                AssertTag(tracingCtx, "path", Is.EqualTo("/delay/1s"));
                AssertTag(tracingCtx, "response.statusCode", Is.Not.Null.And.EqualTo(200));
                AssertTag(tracingCtx, "response.timestamp", Is.Not.Null.And.EqualTo(tracingCtx.ResponseEndTimestamp));
            });


        }

        private static void AssertTag(HttpRequestTracingContext context, string name, IConstraint constraint)
        {
            Assert.That(context.Tags[name], constraint, name);

        }
    }
}