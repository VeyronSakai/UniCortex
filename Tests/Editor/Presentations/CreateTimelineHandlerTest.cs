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
    internal sealed class CreateTimelineHandlerTest
    {
        [Test]
        public void Handle_Returns200_WhenValid()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new CreateTimelineUseCase(dispatcher, ops);
            var handler = new CreateTimelineHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineCreate,
                "{\"instanceId\":12345,\"assetPath\":\"Assets/Test.playable\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("Assets/Test.playable", context.ResponseBody);
            Assert.AreEqual(12345, ops.LastCreateInstanceId);
        }

        [Test]
        public void Handle_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new CreateTimelineUseCase(dispatcher, ops);
            var handler = new CreateTimelineHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineCreate, "");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
        }

        [Test]
        public void Handle_Returns400_WhenAssetPathMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new CreateTimelineUseCase(dispatcher, ops);
            var handler = new CreateTimelineHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TimelineCreate,
                "{\"instanceId\":12345}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("assetPath", context.ResponseBody);
        }
    }
}
