using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class RequestRouterTest
    {
        [Test]
        public void HandleRequestAsync_MainThreadNotResponding_Returns504WithMessage()
        {
            // Arrange
            var router = new RequestRouter();
            router.Register(HttpMethodType.Get, "/test", (_, _) =>
                Task.FromException(new MainThreadNotRespondingException(TimeSpan.FromSeconds(7))));
            var context = new FakeRequestContext(HttpMethodType.Get, "/test");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.GatewayTimeout, context.ResponseStatusCode);
            StringAssert.Contains("has not responded for 7 seconds", context.ResponseBody);
        }
    }
}
