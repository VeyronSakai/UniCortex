using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SelectProjectViewAssetUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsSelectAsset_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyProjectViewOperations();
            var useCase = new SelectProjectViewAssetUseCase(dispatcher, operations);

            useCase.ExecuteAsync("Assets/Scenes/SampleScene.unity", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.SelectAssetCallCount);
            Assert.AreEqual("Assets/Scenes/SampleScene.unity", operations.LastSelectedAssetPath);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
