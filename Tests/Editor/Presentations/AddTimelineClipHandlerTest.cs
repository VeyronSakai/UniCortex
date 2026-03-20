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
    internal sealed class AddTimelineClipHandlerTest
    {
        [Test]
        public void Handle_Returns200_WhenValid()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new AddTimelineClipUseCase(dispatcher, ops);
            var handler = new AddTimelineClipHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var request = new AddTimelineClipRequest
                { instanceId = 12345, trackIndex = 0, start = 1.0, duration = 3.0, clipName = "MyClip" };
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineAddClip,
                JsonUtility.ToJson(request));

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual(0, ops.LastAddClipTrackIndex);
            Assert.AreEqual(1.0, ops.LastAddClipStart, 0.001);
        }

        [Test]
        public void Handle_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new AddTimelineClipUseCase(dispatcher, ops);
            var handler = new AddTimelineClipHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineAddClip, "");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
        }
    }
}
