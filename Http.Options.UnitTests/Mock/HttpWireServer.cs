using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Metrics.Utils;
using WireMock;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Types;
using WireMock.Util;

namespace Http.Options.UnitTests.Mock
{
    public class HttpMockModel
    {
        public string Id;
        public double Timestamp;
    }
    public class HttpWireServer: IDisposable
    { 
        private readonly WireMockServer _server;

        public static HttpWireServer Start()
        {
            return new HttpWireServer();
        }

        
        public HttpWireServer()
        {
            _server = WireMockServer.Start();

            _server
                .Given(Request
                    .Create()
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithDelay(TimeSpan.FromMilliseconds(10))
                    .WithCallback(message => new ResponseMessage()
                    {
                        Headers= { [ "Content-Type"]= new WireMockList<string>()
                        {
                            "application/json"
                        }}, 
                        
                        BodyData = new BodyData()
                        {
                            DetectedBodyType = BodyType.Json,

                            BodyAsJson = new HttpMockModel
                            {
                    
                                Id = message.Query["id"].ToString(),
                                Timestamp = Guid.NewGuid().ToString().GetHashCode()
                    
                            }
                        }
                    })
               
                    
                    );
                  

         
            Console.WriteLine($"Hades Mock listening on 127.0.0.1:{_server.Ports.First()}");
        }


        public void ConfigureHttp (HttpClientOptions options)
        {
            options.Connection = Connection();
        }

        public HttpConnectionOptions  Connection()
        {
            return new HttpConnectionOptions()
            {
                Server = "127.0.0.1",
                Port = _server.Ports.First(),
                Schema = "http"
            };
        }

        public void Dispose()
        {
            _server?.Dispose();
        }
    

    }
}