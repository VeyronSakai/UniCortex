using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Timeline;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class ModifyTimelineClipHandlerTest
    {
        private SpyTimelineOperations _ops;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            _ops = new SpyTimelineOperations();
            var handler = new ModifyTimelineClipHandler(new ModifyTimelineClipUseCase(new FakeMainThreadDispatcher(), _ops));
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void Handle_PassesOnlyGivenFields()
        {
            // Arrange
            const string body =
                "{\"instanceId\":12345,\"trackIndex\":1,\"clipIndex\":2,\"start\":0,\"easeInDuration\":0.25,\"postExtrapolation\":\"Loop\"}";
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineModifyClip, body);

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            var request = _ops.LastModifyClipRequest;
            Assert.AreEqual(12345, request.instanceId);
            Assert.AreEqual(1, request.trackIndex);
            Assert.AreEqual(2, request.clipIndex);
            Assert.AreEqual(0.0, request.start);
            Assert.AreEqual(0.25, request.easeInDuration);
            Assert.AreEqual("Loop", request.postExtrapolation);
            Assert.IsNull(request.duration);
            Assert.IsNull(request.timeScale);
            Assert.IsNull(request.clipIn);
            Assert.IsNull(request.easeOutDuration);
            Assert.IsNull(request.preExtrapolation);
            Assert.IsNull(request.displayName);
        }

        [Test]
        public void Handle_Returns400_WhenBodyEmpty()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineModifyClip, "");

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, _ops.ModifyClipCallCount);
        }

        [Test]
        public void Handle_Returns400_WhenInstanceIdMissing()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineModifyClip,
                "{\"trackIndex\":0,\"clipIndex\":0,\"start\":1}");

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, _ops.ModifyClipCallCount);
        }
    }
}
