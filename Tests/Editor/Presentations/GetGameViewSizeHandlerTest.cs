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
    internal sealed class GetGameViewSizeHandlerTest
    {
        [Test]
        public void HandleGetGameViewSize_Returns200WithScreenSize()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyEditorWindowOperations { GameViewWidth = 1920, GameViewHeight = 1080 };
            var useCase = new GetGameViewSizeUseCase(dispatcher, operations);
            var handler = new GetGameViewSizeHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.GameViewSize);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("1920", context.ResponseBody);
            StringAssert.Contains("1080", context.ResponseBody);
            Assert.AreEqual(1, operations.GetGameViewSizeCallCount);
        }
    }
}
