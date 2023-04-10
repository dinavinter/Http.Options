using System;
using System.Collections.Generic;
using System.Linq;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Http.Options.UnitTests
{
    public class WireServer:IDisposable
    {
        private readonly WireMockServer _server;

        private IEnumerable<(string path, double weight, TimeSpan delay, int statusCode)> _maps =
            new (string path, double weight, TimeSpan delay, int statusCode)[]
            {
                ("/timeout/1s", 0.1, TimeSpan.FromSeconds(1), 408),
                ("/delay/1s", 0.01, TimeSpan.FromSeconds(1), 200),
                ("/delay/2s", 0.1, TimeSpan.FromSeconds(2), 200),
                ("/delay/5s", 0.1, TimeSpan.FromSeconds(5), 200),
                ("/delay/10ms", 0.4, TimeSpan.FromMilliseconds(10), 200),
                ("/delay/5ms", 0.2, TimeSpan.FromMilliseconds(5), 200),
                ("/delay/200ms", 0.6, TimeSpan.FromMilliseconds(200), 200),
                ("/delay/300ms", 0.4, TimeSpan.FromMilliseconds(300), 200),
                ("/error/5ms", 0.01, TimeSpan.FromMilliseconds(5), 500),
                ("/error/5s", 0.01, TimeSpan.FromSeconds(5), 500),

            };
        public WireServer(WireMockServer server)
        {
            _server = server;
            foreach (var map in _maps)
            {
                _server
                    .Given(Request.Create()
                        .WithPath(map.path)
                        .UsingGet())
                    .RespondWith(Response.Create()
                        .WithStatusCode(map.statusCode).WithDelay(map.delay)
                        .WithBodyAsJson(new
                        {
                            booo = "abc",
                            bla = "uupodsodp"
                        })
                        .WithHeaders(new Dictionary<string, string>()
                        {
                            ["Content-Length"] = "50"
                        }))
                    ;

            }
        }

        
        public void ConfigureWireMockServer(HttpClientOptions options)
        {
            options.Connection.Server = "127.0.0.1";
            options.Connection.Schema = "http";
            options.Connection.Port = _server.Ports.First(); 

        }

        public string  Url(string path)
        {
            return $"http://127.0.0.1:{_server.Ports.First()}{path}";
        }

        public void Dispose()
        {
            _server?.Dispose();
        }
    }
}