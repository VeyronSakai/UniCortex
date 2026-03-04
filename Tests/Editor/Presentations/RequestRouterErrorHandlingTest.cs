using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class RequestRouterErrorHandlingTest
    {
        [Test]
        public void HandleRequestAsync_Returns405_WhenMethodIsNotSupported()
        {
            var router = new RequestRouter();
            var context = new FakeRequestContext("PATCH", "/editor/ping");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.MethodNotAllowed, context.ResponseStatusCode);
            StringAssert.Contains("Method not allowed", context.ResponseBody);
        }

        [Test]
        public void HandleRequestAsync_Returns400_WhenHandlerThrowsArgumentException()
        {
            var router = new RequestRouter();
            router.Register(HttpMethodType.Post, "/test/invalid-body",
                (_, _) => Task.FromException(new ArgumentException("Unexpected token at position 3")));

            var context = new FakeRequestContext("POST", "/test/invalid-body");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("Invalid request body.", context.ResponseBody);
            StringAssert.DoesNotContain("Unexpected token", context.ResponseBody);
        }

        [Test]
        public void HandleRequestAsync_Returns500WithoutLeakingExceptionMessage()
        {
            var router = new RequestRouter();
            router.Register(HttpMethodType.Get, "/test/failure",
                (_, _) => Task.FromException(new InvalidOperationException("sensitive internal details")));

            var context = new FakeRequestContext("GET", "/test/failure");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.InternalServerError, context.ResponseStatusCode);
            StringAssert.Contains("Internal server error", context.ResponseBody);
            StringAssert.DoesNotContain("sensitive internal details", context.ResponseBody);
        }
    }
}
