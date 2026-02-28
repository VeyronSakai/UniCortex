using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;
using UniCortex.Editor.Handlers.Editor;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class RedoHandlerTest
    {
        [Test]
        public void HandleRedo_Returns200WithSuccessResponse()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var undo = new SpyUndo();
            var useCase = new RedoUseCase(dispatcher, undo);
            var handler = new RedoHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.Redo);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("success", context.ResponseBody);
            Assert.AreEqual(1, undo.PerformRedoCallCount);
            Assert.AreEqual(0, undo.PerformUndoCallCount);
        }
    }
}
