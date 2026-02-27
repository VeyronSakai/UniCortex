using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Scene;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class SaveSceneHandlerTest
    {
        [Test]
        public void HandleSaveScene_Returns200WithSuccess()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var useCase = new SaveSceneUseCase(dispatcher, sceneManager);
            var handler = new SaveSceneHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext
            {
                HttpMethod = "POST",
                Path = ApiRoutes.SceneSave
            };

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual(1, sceneManager.SaveOpenScenesCallCount);
        }
    }
}
