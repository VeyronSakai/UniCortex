using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Prefab;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class ClosePrefabHandlerTest
    {
        [Test]
        public void HandleClose_Returns200()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPrefabOperations();
            var useCase = new ClosePrefabUseCase(dispatcher, operations, new SpyEditorSceneManager());
            var handler = new ClosePrefabHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.PrefabClose);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual(1, operations.ClosePrefabCallCount);
            Assert.IsFalse(operations.LastCloseDiscardChanges);
        }

        [Test]
        public void HandleClose_WithDiscardChanges_PassesItToClosePrefab()
        {
            // Arrange
            var operations = new SpyPrefabOperations();
            var sceneManager = new SpyEditorSceneManager { DirtyPrefabStagePath = "Assets/Prefabs/Open.prefab" };
            var useCase = new ClosePrefabUseCase(new FakeMainThreadDispatcher(), operations, sceneManager);
            var router = new RequestRouter();
            new ClosePrefabHandler(useCase).Register(router);
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.PrefabClose,
                "{\"discardChanges\":true}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.IsTrue(operations.LastCloseDiscardChanges);
        }

        [Test]
        public void HandleClose_WhenPrefabHasUnsavedChanges_Returns400WithMessage()
        {
            // Arrange
            var operations = new SpyPrefabOperations();
            var sceneManager = new SpyEditorSceneManager { DirtyPrefabStagePath = "Assets/Prefabs/Open.prefab" };
            var useCase = new ClosePrefabUseCase(new FakeMainThreadDispatcher(), operations, sceneManager);
            var router = new RequestRouter();
            new ClosePrefabHandler(useCase).Register(router);
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.PrefabClose);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("unsaved changes", context.ResponseBody);
            Assert.AreEqual(0, operations.ClosePrefabCallCount);
        }
    }
}
