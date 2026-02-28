using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class OpenSceneUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsOpenScene_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var useCase = new OpenSceneUseCase(dispatcher, sceneManager);

            useCase.ExecuteAsync("Assets/Scenes/Main.unity", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, sceneManager.OpenSceneCallCount);
            Assert.AreEqual("Assets/Scenes/Main.unity", sceneManager.LastScenePath);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
