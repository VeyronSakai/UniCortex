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
    internal sealed class BindTimelineTrackHandlerTest
    {
        [Test]
        public void Handle_Returns200_WhenValid()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new BindTimelineTrackUseCase(dispatcher, ops);
            var handler = new BindTimelineTrackHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var request = new BindTimelineTrackRequest
                { instanceId = 12345, trackIndex = 1, targetInstanceId = 67890 };
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineBindTrack,
                JsonUtility.ToJson(request));

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual(1, ops.LastBindTrackTrackIndex);
            Assert.AreEqual(67890, ops.LastBindTrackTargetInstanceId);
        }

        [Test]
        public void Handle_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new BindTimelineTrackUseCase(dispatcher, ops);
            var handler = new BindTimelineTrackHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineBindTrack, "");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
        }

        [Test]
        public void Handle_Returns400_WhenTargetInstanceIdMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new BindTimelineTrackUseCase(dispatcher, ops);
            var handler = new BindTimelineTrackHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var request = new BindTimelineTrackRequest { instanceId = 12345, trackIndex = 1 };
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineBindTrack,
                JsonUtility.ToJson(request));

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("targetInstanceId", context.ResponseBody);
        }
    }
}
