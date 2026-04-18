using System;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class RequestExceptionResponderTest
    {
        [Test]
        public void RespondAsync_Returns408_WhenOperationCancelled()
        {
            var context = new FakeRequestContext(HttpMethodType.Post, "/tests/run");

            RequestExceptionResponder.RespondAsync(context, new OperationCanceledException())
                .GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.RequestTimeout, context.ResponseStatusCode);
            StringAssert.Contains(ErrorMessages.RequestWasCancelled, context.ResponseBody);
        }

        [Test]
        public void RespondAsync_Returns500_WhenUnhandledException()
        {
            var context = new FakeRequestContext(HttpMethodType.Post, "/tests/run");

            RequestExceptionResponder.RespondAsync(context, new InvalidOperationException("Boom"))
                .GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.InternalServerError, context.ResponseStatusCode);
            StringAssert.Contains("Internal server error", context.ResponseBody);
        }
    }
}
