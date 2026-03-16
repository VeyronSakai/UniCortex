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
    internal sealed class EditorStatusHandlerTest
    {
        [Test]
        public void HandleGetStatus_Returns200WithPlayingAndPaused()
        {
            var cache = new EditorStateCache();
            cache.UpdatePlayModeState(true);
            cache.UpdatePauseState(true);
            var useCase = new GetEditorStatusUseCase(cache);
            var handler = new EditorStatusHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.Status);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
        }

        [Test]
        public void HandleGetStatus_WhenNotPlaying_Returns200WithFalse()
        {
            var cache = new EditorStateCache();
            var useCase = new GetEditorStatusUseCase(cache);
            var handler = new EditorStatusHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.Status);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("false", context.ResponseBody);
        }
    }
}
