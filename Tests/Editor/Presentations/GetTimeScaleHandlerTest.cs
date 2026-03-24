using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Editor;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class GetTimeScaleHandlerTest
    {
        [Test]
        public void HandleGetTimeScale_Returns200WithCurrentTimeScale()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var timeOps = new SpyTimeOperations { TimeScale = 2f };
            var useCase = new GetTimeScaleUseCase(dispatcher, timeOps);
            var handler = new GetTimeScaleHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.TimeScale);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("2", context.ResponseBody);
        }
    }
}
