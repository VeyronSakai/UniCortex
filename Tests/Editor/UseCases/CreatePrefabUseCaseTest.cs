using System.Threading;
using UniCortex.Editor.Domains.Models;
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

            useCase.ExecuteAsync(12345, "Assets/Prefabs/Test.prefab", cancellationToken: CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.CreatePrefabCallCount);
            Assert.AreEqual(12345, operations.LastCreateInstanceId);
            Assert.AreEqual("Assets/Prefabs/Test.prefab", operations.LastCreateAssetPath);
            Assert.IsFalse(operations.LastCreateConnect);
            Assert.AreEqual(1, dispatcher.CallCount);
        }

        [Test]
        public void ExecuteAsync_WithConnect_PassesConnect_And_ReturnsResult()
        {
            // Arrange
            var operations = new SpyPrefabOperations
            {
                CreateResult = new CreatePrefabResponse(true, "0123456789abcdef0123456789abcdef")
            };
            var useCase = new CreatePrefabUseCase(new FakeMainThreadDispatcher(), operations);

            // Act
            var result = useCase.ExecuteAsync(12345, "Assets/Prefabs/Test.prefab", true, CancellationToken.None)
                .GetAwaiter().GetResult();

            // Assert
            Assert.IsTrue(operations.LastCreateConnect);
            Assert.AreEqual("0123456789abcdef0123456789abcdef", result.guid);
        }
    }
}
