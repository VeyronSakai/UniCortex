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
    internal sealed class DomainReloadHandlerTest
    {
        [Test]
        public void HandleDomainReload_Returns200_WhenCompilationSucceeds()
        {
            // Arrange
            var compilationPipeline = new SpyCompilationPipeline { CompilationSucceeds = true };
            var useCase = new RequestDomainReloadUseCase(new FakeMainThreadDispatcher(), compilationPipeline,
                new SpyEditorApplication());
            var handler = new DomainReloadHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.DomainReload);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual(1, compilationPipeline.RequestScriptCompilationCallCount);
        }

        [Test]
        public void HandleDomainReload_Returns400_WhenCompilationFails()
        {
            // Arrange
            var compilationPipeline = new SpyCompilationPipeline { CompilationSucceeds = false };
            var useCase = new RequestDomainReloadUseCase(new FakeMainThreadDispatcher(), compilationPipeline,
                new SpyEditorApplication());
            var handler = new DomainReloadHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.DomainReload);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("compilation failed", context.ResponseBody);
        }

        [Test]
        public void HandleDomainReload_Returns400_WhenInPlayMode()
        {
            // Arrange
            var compilationPipeline = new SpyCompilationPipeline();
            var useCase = new RequestDomainReloadUseCase(new FakeMainThreadDispatcher(), compilationPipeline,
                new SpyEditorApplication { IsPlaying = true });
            var handler = new DomainReloadHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.DomainReload);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("play mode", context.ResponseBody);
            Assert.AreEqual(0, compilationPipeline.RequestScriptCompilationCallCount);
        }
    }
}
