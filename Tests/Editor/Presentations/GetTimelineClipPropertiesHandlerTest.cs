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
    internal sealed class GetTimelineClipPropertiesHandlerTest
    {
        private SpyTimelineOperations _ops;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            _ops = new SpyTimelineOperations();
            var handler = new GetTimelineClipPropertiesHandler(
                new GetTimelineClipPropertiesUseCase(new FakeMainThreadDispatcher(), _ops));
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void Handle_Returns200_WhenValid()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.TimelineClipProperties);
            context.SetQueryParameter("assetPath", "Assets/My.playable");
            context.SetQueryParameter("trackIndex", "1");
            context.SetQueryParameter("clipIndex", "2");

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("UnityEngine.Timeline.AnimationPlayableAsset", context.ResponseBody);
            Assert.AreEqual(0, _ops.LastGetClipPropertiesInstanceId);
            Assert.AreEqual("Assets/My.playable", _ops.LastGetClipPropertiesAssetPath);
            Assert.AreEqual(1, _ops.LastGetClipPropertiesTrackIndex);
            Assert.AreEqual(2, _ops.LastGetClipPropertiesClipIndex);
        }

        [Test]
        public void Handle_Returns400_WhenClipIndexMissing()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.TimelineClipProperties);
            context.SetQueryParameter("instanceId", "12345");
            context.SetQueryParameter("trackIndex", "0");

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, _ops.GetClipPropertiesCallCount);
        }

        [Test]
        public void Handle_Returns400_WhenClipIndexIsNotInteger()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.TimelineClipProperties);
            context.SetQueryParameter("instanceId", "12345");
            context.SetQueryParameter("trackIndex", "0");
            context.SetQueryParameter("clipIndex", "abc");

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, _ops.GetClipPropertiesCallCount);
        }
    }
}
