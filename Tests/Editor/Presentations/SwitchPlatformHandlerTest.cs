using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Editor;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class SwitchPlatformHandlerTest
    {
        [Test]
        public void HandleSwitchPlatform_Returns200WithBuildTargets()
        {
            // Arrange
            var operations = new SpyBuildTargetOperations { ActiveBuildTarget = "StandaloneOSX" };
            var router = CreateRouter(operations, new SpyEditorApplication());
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.PlatformSwitch,
                "{\"buildTarget\":\"Android\"}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("\"previousBuildTarget\":\"StandaloneOSX\"", context.ResponseBody);
            StringAssert.Contains("\"activeBuildTarget\":\"Android\"", context.ResponseBody);
            CollectionAssert.AreEqual(new[] { "Android" }, operations.SwitchedBuildTargets);
        }

        [TestCase("")]
        [TestCase("{}")]
        public void HandleSwitchPlatform_Returns400_WhenBuildTargetMissing(string body)
        {
            // Arrange
            var router = CreateRouter(new SpyBuildTargetOperations(), new SpyEditorApplication());
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.PlatformSwitch, body);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("buildTarget is required", context.ResponseBody);
        }

        [Test]
        public void HandleSwitchPlatform_Returns400_WhenBuildTargetUnknown()
        {
            // Arrange
            var router = CreateRouter(new SpyBuildTargetOperations(), new SpyEditorApplication());
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.PlatformSwitch,
                "{\"buildTarget\":\"Unknown\"}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("Unknown build target", context.ResponseBody);
        }

        [Test]
        public void HandleSwitchPlatform_Returns400_WhenInPlayMode()
        {
            // Arrange
            var operations = new SpyBuildTargetOperations();
            var router = CreateRouter(operations, new SpyEditorApplication { IsPlaying = true });
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.PlatformSwitch,
                "{\"buildTarget\":\"Android\"}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("play mode", context.ResponseBody);
            Assert.IsEmpty(operations.SwitchedBuildTargets);
        }

        [Test]
        public void HandleSwitchPlatform_Returns400_WhenSwitchFails()
        {
            // Arrange
            var router = CreateRouter(new SpyBuildTargetOperations { SwitchSucceeds = false },
                new SpyEditorApplication());
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.PlatformSwitch,
                "{\"buildTarget\":\"Android\"}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("Failed to switch platform", context.ResponseBody);
        }

        private static RequestRouter CreateRouter(SpyBuildTargetOperations operations,
            SpyEditorApplication editorApplication)
        {
            var useCase = new SwitchPlatformUseCase(new FakeMainThreadDispatcher(), operations, editorApplication);
            var handler = new SwitchPlatformHandler(useCase);
            var router = new RequestRouter();
            handler.Register(router);
            return router;
        }
    }
}
