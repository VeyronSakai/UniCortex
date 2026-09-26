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
    internal sealed class GetTimelineTracksHandlerTest
    {
        private SpyTimelineOperations _ops;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            _ops = new SpyTimelineOperations();
            var handler = new GetTimelineTracksHandler(new GetTimelineTracksUseCase(new FakeMainThreadDispatcher(), _ops));
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void Handle_Returns200_WhenInstanceIdGiven()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.TimelineTracks);
            context.SetQueryParameter("instanceId", "12345");

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("Assets/Test.playable", context.ResponseBody);
            Assert.AreEqual(12345, _ops.LastGetTracksInstanceId);
        }

        [Test]
        public void Handle_Returns200_WhenAssetPathGiven()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.TimelineTracks);
            context.SetQueryParameter("assetPath", "Assets/My.playable");

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(0, _ops.LastGetTracksInstanceId);
            Assert.AreEqual("Assets/My.playable", _ops.LastGetTracksAssetPath);
        }

        [Test]
        public void Handle_Returns400_WhenNeitherInstanceIdNorAssetPathGiven()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.TimelineTracks);

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, _ops.GetTracksCallCount);
        }

        [Test]
        public void Handle_Returns400_WhenInstanceIdIsNotInteger()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.TimelineTracks);
            context.SetQueryParameter("instanceId", "abc");

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, _ops.GetTracksCallCount);
        }
    }
}
