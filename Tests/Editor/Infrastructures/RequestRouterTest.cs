using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class RequestRouterTest
    {
        [Test]
        public void HandleRequestAsync_Returns503_WhenMainThreadUnresponsive()
        {
            // Arrange
            var router = new RequestRouter();
            var exception = new MainThreadUnresponsiveException(TimeSpan.FromSeconds(30));
            router.Register(HttpMethodType.Get, ApiRoutes.Ping,
                (_, _) => Task.FromException(exception));
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.Ping);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.ServiceUnavailable, context.ResponseStatusCode);
            StringAssert.Contains("modal dialog", context.ResponseBody);
        }
    }
}
