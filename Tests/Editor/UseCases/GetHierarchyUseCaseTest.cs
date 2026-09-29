using System.Collections.Generic;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetHierarchyUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsHierarchy_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            sceneManager.HierarchyResult = new GetHierarchyResponse(new List<SceneHierarchy>
            {
                new SceneHierarchy("TestScene", "Assets/Scenes/TestScene.unity", true,
                    new List<GameObjectNode>
                    {
                        new GameObjectNode("Camera", 100, new List<GameObjectNode>())
                    }),
                new SceneHierarchy("AdditiveScene", "Assets/Scenes/AdditiveScene.unity", false,
                    new List<GameObjectNode>())
            });
            var useCase = new GetHierarchyUseCase(dispatcher, sceneManager);

            var result = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(2, result.scenes.Count);
            Assert.AreEqual("TestScene", result.scenes[0].sceneName);
            Assert.AreEqual("Assets/Scenes/TestScene.unity", result.scenes[0].scenePath);
            Assert.IsTrue(result.scenes[0].isActive);
            Assert.AreEqual(1, result.scenes[0].gameObjects.Count);
            Assert.AreEqual("Camera", result.scenes[0].gameObjects[0].name);
            Assert.AreEqual("AdditiveScene", result.scenes[1].sceneName);
            Assert.IsFalse(result.scenes[1].isActive);
            Assert.AreEqual(1, sceneManager.GetHierarchyCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
