using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using UniCortex.Editor.Handlers.Timeline;
using NUnit.Framework;
using UnityEngine;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class EvaluateTimelineHandlerTest
    {
        [Test]
        public void Handle_Returns200_WhenValid()
        {
            // Arrange
            var ops = new SpyTimelineOperations();
            var router = CreateRouter(ops);
            var request = new EvaluateTimelineRequest { instanceId = 12345, time = 2.5 };
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineEvaluate,
                JsonUtility.ToJson(request));

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual(1, ops.EvaluateCallCount);
            Assert.AreEqual(12345, ops.LastEvaluateInstanceId);
            Assert.AreEqual(2.5, ops.LastEvaluateTime);
        }

        [Test]
        public void Handle_Returns400_WhenBodyEmpty()
        {
            // Arrange
            var ops = new SpyTimelineOperations();
            var router = CreateRouter(ops);
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineEvaluate, "");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, ops.EvaluateCallCount);
        }

        [Test]
        public void Handle_Returns400_WhenTimeIsNegative()
        {
            // Arrange
            var ops = new SpyTimelineOperations();
            var router = CreateRouter(ops);
            var request = new EvaluateTimelineRequest { instanceId = 12345, time = -1.0 };
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineEvaluate,
                JsonUtility.ToJson(request));

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, ops.EvaluateCallCount);
        }

        private static RequestRouter CreateRouter(SpyTimelineOperations ops)
        {
            var useCase = new EvaluateTimelineUseCase(new FakeMainThreadDispatcher(), ops);
            var handler = new EvaluateTimelineHandler(useCase);
            var router = new RequestRouter();
            handler.Register(router);
            return router;
        }
    }
}
