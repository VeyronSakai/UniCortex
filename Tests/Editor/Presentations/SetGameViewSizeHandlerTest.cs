using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.GameView;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;
using UnityEngine;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class SetGameViewSizeHandlerTest
    {
        [Test]
        public void HandleSetGameViewSize_WithIndex_Returns200()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyEditorWindowOperations();
            var useCase = new SetGameViewSizeUseCase(dispatcher, operations);
            var handler = new SetGameViewSizeHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var request = new SetGameViewSizeRequest { index = 2 };
            var body = JsonUtility.ToJson(request);
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.GameViewSize, body);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(2, operations.LastSetIndex);
            Assert.AreEqual(1, operations.SetGameViewSizeByIndexCallCount);
            StringAssert.Contains("true", context.ResponseBody);
        }

        [Test]
        public void HandleSetGameViewSize_WithWidthAndHeight_Returns200()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyEditorWindowOperations();
            var useCase = new SetGameViewSizeUseCase(dispatcher, operations);
            var handler = new SetGameViewSizeHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var request = new SetGameViewSizeRequest { width = 1920, height = 1080 };
            var body = JsonUtility.ToJson(request);
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.GameViewSize, body);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(1920, operations.GameViewWidth);
            Assert.AreEqual(1080, operations.GameViewHeight);
            Assert.AreEqual(1, operations.SetGameViewSizeCallCount);
            StringAssert.Contains("true", context.ResponseBody);
        }
    }
}
