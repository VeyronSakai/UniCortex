using System;
using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Prefab;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class UnpackPrefabInstanceHandlerTest
    {
        private static RequestRouter CreateRouter(SpyPrefabOperations operations)
        {
            var useCase = new UnpackPrefabInstanceUseCase(new FakeMainThreadDispatcher(), operations);
            var router = new RequestRouter();
            new UnpackPrefabInstanceHandler(useCase).Register(router);
            return router;
        }

        [Test]
        public void HandleUnpack_Returns200_AndPassesInstanceIdAndMode()
        {
            // Arrange
            var operations = new SpyPrefabOperations();
            var router = CreateRouter(operations);
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.PrefabUnpack,
                "{\"instanceId\":12345,\"mode\":\"Completely\"}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual(12345, operations.LastUnpackInstanceId);
            Assert.AreEqual(PrefabUnpackModes.Completely, operations.LastUnpackMode);
        }

        [Test]
        public void HandleUnpack_WithoutMode_UsesOutermostRoot()
        {
            // Arrange
            var operations = new SpyPrefabOperations();
            var router = CreateRouter(operations);
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.PrefabUnpack,
                "{\"instanceId\":12345}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(PrefabUnpackModes.OutermostRoot, operations.LastUnpackMode);
        }

        [TestCase("")]
        [TestCase("{}")]
        public void HandleUnpack_WithoutInstanceId_Returns400(string body)
        {
            // Arrange
            var operations = new SpyPrefabOperations();
            var router = CreateRouter(operations);
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.PrefabUnpack, body);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("instanceId is required", context.ResponseBody);
            Assert.AreEqual(0, operations.UnpackCallCount);
        }

        [Test]
        public void HandleUnpack_WithUnknownMode_Returns400()
        {
            // Arrange
            var operations = new SpyPrefabOperations();
            var router = CreateRouter(operations);
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.PrefabUnpack,
                "{\"instanceId\":12345,\"mode\":\"Partially\"}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("Unknown unpack mode", context.ResponseBody);
            Assert.AreEqual(0, operations.UnpackCallCount);
        }

        [Test]
        public void HandleUnpack_WhenNotOutermostPrefabInstanceRoot_Returns400WithMessage()
        {
            // Arrange
            var operations = new SpyPrefabOperations
            {
                UnpackException = new ArgumentException("'Child' (instanceId 12345) is not a prefab instance.")
            };
            var router = CreateRouter(operations);
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.PrefabUnpack,
                "{\"instanceId\":12345}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("is not a prefab instance", context.ResponseBody);
        }
    }
}
