using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.SceneView;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class FocusSceneViewHandlerTest
    {
        [Test]
        public void HandleFocusSceneView_Returns200WithSuccess()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyEditorWindowOperations();
            var useCase = new FocusSceneViewUseCase(dispatcher, operations);
            var handler = new FocusSceneViewHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.FocusSceneView);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual(1, operations.FocusSceneViewCallCount);
        }
    }
}
