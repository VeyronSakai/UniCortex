using System.Collections.Generic;
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
    internal sealed class HierarchyHandlerTest
    {
        [Test]
        public void HandleHierarchy_Returns200WithHierarchy()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            sceneManager.HierarchyResult = new GetHierarchyResponse(new List<SceneHierarchy>
            {
                new SceneHierarchy("SampleScene", "Assets/Scenes/SampleScene.unity", true,
                    new List<GameObjectNode>
                    {
                        new GameObjectNode("Main Camera", 100, new List<GameObjectNode>())
                    })
            });
            var useCase = new GetHierarchyUseCase(dispatcher, sceneManager);
            var handler = new HierarchyHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.Hierarchy);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("\"scenes\":[{\"sceneName\":\"SampleScene\"", context.ResponseBody);
            StringAssert.Contains("\"isActive\":true", context.ResponseBody);
            StringAssert.Contains("Main Camera", context.ResponseBody);
            Assert.AreEqual(1, sceneManager.GetHierarchyCallCount);
        }
    }
}
