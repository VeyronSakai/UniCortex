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
    internal sealed class SceneHierarchyHandlerTest
    {
        [Test]
        public void HandleSceneHierarchy_Returns200WithHierarchy()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            sceneManager.HierarchyResult = new SceneHierarchyResponse("SampleScene", "Assets/Scenes/SampleScene.unity",
                new List<GameObjectNode>
                {
                    new GameObjectNode("Main Camera", 100, true,
                        new List<string> { "Transform", "Camera" },
                        new List<GameObjectNode>())
                });
            var useCase = new GetSceneHierarchyUseCase(dispatcher, sceneManager);
            var handler = new SceneHierarchyHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext
            {
                HttpMethod = "GET",
                Path = ApiRoutes.SceneHierarchy
            };

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("SampleScene", context.ResponseBody);
            StringAssert.Contains("Main Camera", context.ResponseBody);
            Assert.AreEqual(1, sceneManager.GetHierarchyCallCount);
        }
    }
}
