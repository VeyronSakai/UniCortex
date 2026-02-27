using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class CreatePrefabUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsCreatePrefab_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPrefabOperations();
            var useCase = new CreatePrefabUseCase(dispatcher, operations);

            useCase.ExecuteAsync(12345, "Assets/Prefabs/Test.prefab", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.CreatePrefabCallCount);
            Assert.AreEqual(12345, operations.LastCreateInstanceId);
            Assert.AreEqual("Assets/Prefabs/Test.prefab", operations.LastCreateAssetPath);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
