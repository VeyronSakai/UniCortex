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
    internal sealed class SetTimelineTrackPropertyHandlerTest
    {
        private SpyTimelineOperations _ops;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            _ops = new SpyTimelineOperations();
            var handler = new SetTimelineTrackPropertyHandler(
                new SetTimelineTrackPropertyUseCase(new FakeMainThreadDispatcher(), _ops));
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void Handle_Returns200_WhenValid()
        {
            // Arrange
            var request = new SetTimelineTrackPropertyRequest
                { instanceId = 12345, trackIndex = 1, propertyPath = "m_Muted", value = "true" };
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineSetTrackProperty,
                JsonUtility.ToJson(request));

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(1, _ops.LastSetTrackPropertyTrackIndex);
            Assert.AreEqual("m_Muted", _ops.LastSetTrackPropertyPath);
            Assert.AreEqual("true", _ops.LastSetTrackPropertyValue);
        }

        [Test]
        public void Handle_Returns400_WhenPropertyPathMissing()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineSetTrackProperty,
                "{\"instanceId\":12345,\"trackIndex\":0,\"value\":\"true\"}");

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, _ops.SetTrackPropertyCallCount);
        }

        [Test]
        public void Handle_Returns400_WhenBodyEmpty()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineSetTrackProperty, "");

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
        }
    }
}
