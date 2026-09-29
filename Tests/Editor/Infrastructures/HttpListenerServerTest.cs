using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class HttpListenerServerTest
    {
        private const string SlowRoute = "/test/slow";

        private HttpListenerServer _server;
        private HttpClient _client;
        private int _port;

        [SetUp]
        public void SetUp()
        {
            _port = FindFreePort();
            _client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        }

        [TearDown]
        public void TearDown()
        {
            _server?.Stop();
            _client.Dispose();
        }

        [Test]
        public void Stop_LetsCancelledRequestRespondBeforeClosing()
        {
            // Arrange
            var handlerEntered = new ManualResetEventSlim();
            var router = new RequestRouter();
            router.Register(HttpMethodType.Get, SlowRoute, async (_, cancellationToken) =>
            {
                handlerEntered.Set();
                try
                {
                    await Task.Delay(Timeout.Infinite, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Simulate work between the cancellation and the error response,
                    // so that closing the listener right away would drop the connection.
                    await Task.Delay(200, CancellationToken.None);
                    throw;
                }
            });
            _server = new HttpListenerServer(router, _port);
            _server.Start();

            // Send from a thread-pool thread so that the continuation does not need the main thread.
            var responseTask = Task.Run(() => _client.GetAsync($"http://localhost:{_port}{SlowRoute}"));
            Assert.IsTrue(handlerEntered.Wait(TimeSpan.FromSeconds(5)), "The request did not reach the handler.");

            // Act
            _server.Stop();
            _server = null;

            // Assert
            using var response = responseTask.GetAwaiter().GetResult();
            Assert.AreEqual(HttpStatusCode.RequestTimeout, response.StatusCode);
        }

        private static int FindFreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
