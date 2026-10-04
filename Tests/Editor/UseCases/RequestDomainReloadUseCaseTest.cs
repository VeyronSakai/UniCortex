using System.Threading;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class RequestDomainReloadUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsRequestScriptCompilation_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var compilationPipeline = new SpyCompilationPipeline();
            var useCase = new RequestDomainReloadUseCase(dispatcher, compilationPipeline, new SpyEditorApplication());

            useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, compilationPipeline.RequestScriptCompilationCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }

        [Test]
        public void ExecuteAsync_ReturnsTrue_WhenCompilationSucceeds()
        {
            // Arrange
            var compilationPipeline = new SpyCompilationPipeline { CompilationSucceeds = true };
            var useCase = new RequestDomainReloadUseCase(new FakeMainThreadDispatcher(), compilationPipeline,
                new SpyEditorApplication());

            // Act
            var reloading = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.IsTrue(reloading);
        }

        [Test]
        public void ExecuteAsync_ReturnsFalse_WhenCompilationFails()
        {
            // Arrange
            var compilationPipeline = new SpyCompilationPipeline { CompilationSucceeds = false };
            var useCase = new RequestDomainReloadUseCase(new FakeMainThreadDispatcher(), compilationPipeline,
                new SpyEditorApplication());

            // Act
            var reloading = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.IsFalse(reloading);
        }

        [Test]
        public void ExecuteAsync_ThrowsPlayModeException_WhenInPlayMode()
        {
            // Arrange
            var compilationPipeline = new SpyCompilationPipeline();
            var useCase = new RequestDomainReloadUseCase(new FakeMainThreadDispatcher(), compilationPipeline,
                new SpyEditorApplication { IsPlaying = true });

            // Act & Assert
            Assert.Throws<PlayModeException>(() =>
                useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
            Assert.AreEqual(0, compilationPipeline.RequestScriptCompilationCallCount);
        }
    }
}
