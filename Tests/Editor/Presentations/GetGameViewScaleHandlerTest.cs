using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.GameView;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class GetGameViewScaleHandlerTest
    {
        [Test]
        public void HandleGetGameViewScale_Returns200WithScale()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyEditorWindowOperations { Scale = 1.5f, MinScale = 0.5f, MaxScale = 5.0f };
            var useCase = new GetGameViewScaleUseCase(dispatcher, operations);
            var handler = new GetGameViewScaleHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.GameViewScale);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("1.5", context.ResponseBody);
            Assert.AreEqual(1, operations.GetGameViewScaleCallCount);
        }
    }
}
