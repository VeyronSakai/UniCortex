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
    internal sealed class SetTimelineBindingHandlerTest
    {
        [Test]
        public void Handle_Returns200_WhenValid()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new SetTimelineBindingUseCase(dispatcher, ops);
            var handler = new SetTimelineBindingHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineSetBinding,
                "{\"instanceId\":12345,\"trackIndex\":1,\"targetInstanceId\":67890}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual(1, ops.LastSetBindingTrackIndex);
            Assert.AreEqual(67890, ops.LastSetBindingTargetInstanceId);
        }

        [Test]
        public void Handle_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new SetTimelineBindingUseCase(dispatcher, ops);
            var handler = new SetTimelineBindingHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineSetBinding, "");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
        }

        [Test]
        public void Handle_Returns400_WhenTargetInstanceIdMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new SetTimelineBindingUseCase(dispatcher, ops);
            var handler = new SetTimelineBindingHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineSetBinding,
                "{\"instanceId\":12345,\"trackIndex\":1}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("targetInstanceId", context.ResponseBody);
        }
    }
}
