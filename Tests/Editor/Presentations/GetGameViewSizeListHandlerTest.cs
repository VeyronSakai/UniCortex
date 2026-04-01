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
    internal sealed class GetGameViewSizeListHandlerTest
    {
        [Test]
        public void HandleGetGameViewSizeList_Returns200WithSizes()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyEditorWindowOperations { SelectedSizeIndex = 1 };
            var useCase = new GetGameViewSizeListUseCase(dispatcher, operations);
            var handler = new GetGameViewSizeListHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.GameViewSizeList);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("Free Aspect", context.ResponseBody);
            StringAssert.Contains("1920", context.ResponseBody);
            Assert.AreEqual(1, operations.GetGameViewSizeListCallCount);
        }
    }
}
