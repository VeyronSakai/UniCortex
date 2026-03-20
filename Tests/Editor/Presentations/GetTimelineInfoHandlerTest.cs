using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using UniCortex.Editor.Handlers.Timeline;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class GetTimelineInfoHandlerTest
    {
        [Test]
        public void Handle_Returns200_WhenValid()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new GetTimelineInfoUseCase(dispatcher, ops);
            var handler = new GetTimelineInfoHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.TimelineInfo);
            context.SetQueryParameter("instanceId", "12345");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("TestTimeline", context.ResponseBody);
        }

        [Test]
        public void Handle_Returns400_WhenInstanceIdMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new GetTimelineInfoUseCase(dispatcher, ops);
            var handler = new GetTimelineInfoHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.TimelineInfo);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("instanceId", context.ResponseBody);
        }
    }
}
