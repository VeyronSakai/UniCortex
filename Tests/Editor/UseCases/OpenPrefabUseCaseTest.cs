using System.Threading;
using UniCortex.Editor.Domains.Exceptions;
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
            var useCase = new OpenPrefabUseCase(dispatcher, operations, new SpyEditorSceneManager());

            useCase.ExecuteAsync("Assets/Prefabs/Test.prefab", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.OpenPrefabCallCount);
            Assert.AreEqual("Assets/Prefabs/Test.prefab", operations.LastOpenAssetPath);
            Assert.AreEqual(1, dispatcher.CallCount);
        }

        [Test]
        public void ExecuteAsync_ThrowsUnsavedPrefabChangesException_WhenOpenPrefabHasUnsavedChanges()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPrefabOperations();
            var sceneManager = new SpyEditorSceneManager { DirtyPrefabStagePath = "Assets/Prefabs/Open.prefab" };
            var useCase = new OpenPrefabUseCase(dispatcher, operations, sceneManager);

            // Act
            var ex = Assert.Throws<UnsavedPrefabChangesException>(() =>
                useCase.ExecuteAsync("Assets/Prefabs/Test.prefab", CancellationToken.None)
                    .GetAwaiter().GetResult());

            // Assert
            StringAssert.Contains("Cannot open a Prefab", ex.Message);
            StringAssert.Contains("Assets/Prefabs/Open.prefab", ex.Message);
            Assert.AreEqual(0, operations.OpenPrefabCallCount);
        }
    }
}
