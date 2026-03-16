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
    internal sealed class UnpauseHandlerTest
    {
        [Test]
        public void HandleUnpause_Returns200WithSuccess()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var editorApp = new SpyEditorApplication { IsPaused = true };
            var useCase = new UnpauseUseCase(dispatcher, editorApp);
            var handler = new UnpauseHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.Unpause);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.IsFalse(editorApp.IsPaused);
        }
    }
}
