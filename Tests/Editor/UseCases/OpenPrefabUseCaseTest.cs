using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class OpenPrefabUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsOpenPrefab_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPrefabOperations();
            var useCase = new OpenPrefabUseCase(dispatcher, operations);

            useCase.ExecuteAsync("Assets/Prefabs/Test.prefab", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.OpenPrefabCallCount);
            Assert.AreEqual("Assets/Prefabs/Test.prefab", operations.LastOpenAssetPath);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
