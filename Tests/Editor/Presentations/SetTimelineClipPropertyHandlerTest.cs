using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Timeline;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;
using UnityEngine;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class SetTimelineClipPropertyHandlerTest
    {
        private SpyTimelineOperations _ops;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            _ops = new SpyTimelineOperations();
            var handler = new SetTimelineClipPropertyHandler(
                new SetTimelineClipPropertyUseCase(new FakeMainThreadDispatcher(), _ops));
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void Handle_Returns200_WhenValid()
        {
            // Arrange
            var request = new SetTimelineClipPropertyRequest
                { instanceId = 12345, trackIndex = 1, clipIndex = 2, propertyPath = "postPlayback", value = "Active" };
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineSetClipProperty,
                JsonUtility.ToJson(request));

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(1, _ops.LastSetClipPropertyTrackIndex);
            Assert.AreEqual(2, _ops.LastSetClipPropertyClipIndex);
            Assert.AreEqual("postPlayback", _ops.LastSetClipPropertyPath);
            Assert.AreEqual("Active", _ops.LastSetClipPropertyValue);
        }

        [Test]
        public void Handle_Returns400_WhenPropertyPathMissing()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineSetClipProperty,
                "{\"instanceId\":12345,\"trackIndex\":0,\"clipIndex\":0,\"value\":\"1\"}");

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, _ops.SetClipPropertyCallCount);
        }

        [Test]
        public void Handle_Returns400_WhenBodyEmpty()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineSetClipProperty, "");

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
        }
    }
}
