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
    internal sealed class GetTimelineTrackPropertiesHandlerTest
    {
        private SpyTimelineOperations _ops;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            _ops = new SpyTimelineOperations();
            var handler = new GetTimelineTrackPropertiesHandler(
                new GetTimelineTrackPropertiesUseCase(new FakeMainThreadDispatcher(), _ops));
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void Handle_Returns200_WhenValid()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.TimelineTrackProperties);
            context.SetQueryParameter("instanceId", "12345");
            context.SetQueryParameter("trackIndex", "1");

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("UnityEngine.Timeline.AnimationTrack", context.ResponseBody);
            Assert.AreEqual(12345, _ops.LastGetTrackPropertiesInstanceId);
            Assert.AreEqual(1, _ops.LastGetTrackPropertiesTrackIndex);
        }

        [Test]
        public void Handle_Returns400_WhenTrackIndexMissing()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.TimelineTrackProperties);
            context.SetQueryParameter("instanceId", "12345");

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, _ops.GetTrackPropertiesCallCount);
        }

        [Test]
        public void Handle_Returns400_WhenNeitherInstanceIdNorAssetPathGiven()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.TimelineTrackProperties);
            context.SetQueryParameter("trackIndex", "0");

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, _ops.GetTrackPropertiesCallCount);
        }
    }
}
