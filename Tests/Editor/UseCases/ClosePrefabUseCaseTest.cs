using System.Threading;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class ClosePrefabUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsClosePrefab_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPrefabOperations();
            var useCase = new ClosePrefabUseCase(dispatcher, operations, new SpyEditorSceneManager());

            useCase.ExecuteAsync(false, CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.ClosePrefabCallCount);
            Assert.IsFalse(operations.LastCloseDiscardChanges);
            Assert.AreEqual(1, dispatcher.CallCount);
        }

        [Test]
        public void ExecuteAsync_ThrowsUnsavedPrefabChangesException_WhenPrefabHasUnsavedChanges()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPrefabOperations();
            var sceneManager = new SpyEditorSceneManager { DirtyPrefabStagePath = "Assets/Prefabs/Open.prefab" };
            var useCase = new ClosePrefabUseCase(dispatcher, operations, sceneManager);

            // Act
            var ex = Assert.Throws<UnsavedPrefabChangesException>(() =>
                useCase.ExecuteAsync(false, CancellationToken.None).GetAwaiter().GetResult());

            // Assert
            StringAssert.Contains("Cannot close Prefab Mode", ex.Message);
            StringAssert.Contains("Assets/Prefabs/Open.prefab", ex.Message);
            StringAssert.Contains("discardChanges", ex.Message);
            Assert.AreEqual(0, operations.ClosePrefabCallCount);
        }

        [Test]
        public void ExecuteAsync_DiscardsChanges_WhenAsked()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPrefabOperations();
            var sceneManager = new SpyEditorSceneManager { DirtyPrefabStagePath = "Assets/Prefabs/Open.prefab" };
            var useCase = new ClosePrefabUseCase(dispatcher, operations, sceneManager);

            // Act
            useCase.ExecuteAsync(true, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, operations.ClosePrefabCallCount);
            Assert.IsTrue(operations.LastCloseDiscardChanges);
        }
    }
}
