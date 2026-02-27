using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SaveSceneUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsSaveOpenScenes_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var useCase = new SaveSceneUseCase(dispatcher, sceneManager);

            var result = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsTrue(result);
            Assert.AreEqual(1, sceneManager.SaveOpenScenesCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
