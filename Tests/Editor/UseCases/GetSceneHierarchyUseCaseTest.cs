using System.Collections.Generic;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetSceneHierarchyUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsHierarchy_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            sceneManager.HierarchyResult = new SceneHierarchyResponse("TestScene", "Assets/Scenes/TestScene.unity",
                new List<GameObjectNode>
                {
                    new GameObjectNode("Camera", 100, true, "Untagged", 0, false, false,
                        new List<string> { "Transform", "Camera" }, new List<GameObjectNode>())
                });
            var useCase = new GetSceneHierarchyUseCase(dispatcher, sceneManager);

            var result = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("TestScene", result.sceneName);
            Assert.AreEqual("Assets/Scenes/TestScene.unity", result.scenePath);
            Assert.AreEqual(1, result.gameObjects.Count);
            Assert.AreEqual("Camera", result.gameObjects[0].name);
            Assert.AreEqual(1, sceneManager.GetHierarchyCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
