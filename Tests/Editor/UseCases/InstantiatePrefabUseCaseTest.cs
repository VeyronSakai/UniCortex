using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class InstantiatePrefabUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsResult_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPrefabOperations
            {
                InstantiateResult = new InstantiatePrefabResponse("MyCube", 56789)
            };
            var useCase = new InstantiatePrefabUseCase(dispatcher, operations);

            var result = useCase.ExecuteAsync("Assets/Prefabs/MyCube.prefab", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.InstantiateCallCount);
            Assert.AreEqual("Assets/Prefabs/MyCube.prefab", operations.LastInstantiateAssetPath);
            Assert.AreEqual("MyCube", result.name);
            Assert.AreEqual(56789, result.instanceId);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
