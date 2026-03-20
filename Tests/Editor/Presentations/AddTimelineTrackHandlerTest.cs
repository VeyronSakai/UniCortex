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
    internal sealed class AddTimelineTrackHandlerTest
    {
        [Test]
        public void Handle_Returns200_WhenValid()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new AddTimelineTrackUseCase(dispatcher, ops);
            var handler = new AddTimelineTrackHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var request = new AddTimelineTrackRequest
                { instanceId = 12345, trackType = "UnityEngine.Timeline.AnimationTrack", trackName = "MyTrack" };
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineAddTrack,
                JsonUtility.ToJson(request));

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual("UnityEngine.Timeline.AnimationTrack", ops.LastAddTrackType);
            Assert.AreEqual("MyTrack", ops.LastAddTrackName);
        }

        [Test]
        public void Handle_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new AddTimelineTrackUseCase(dispatcher, ops);
            var handler = new AddTimelineTrackHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineAddTrack, "");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
        }

        [Test]
        public void Handle_Returns400_WhenTrackTypeMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new AddTimelineTrackUseCase(dispatcher, ops);
            var handler = new AddTimelineTrackHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var request = new AddTimelineTrackRequest { instanceId = 12345 };
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineAddTrack,
                JsonUtility.ToJson(request));

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("trackType", context.ResponseBody);
        }
    }
}
