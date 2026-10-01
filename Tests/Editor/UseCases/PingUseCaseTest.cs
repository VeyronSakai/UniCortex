using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class PingUseCaseTest
    {
        [Test]
        public void ExecuteAsync_Verbose_ReturnsMessage_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var useCase = new PingUseCase(dispatcher, new StubEditorDomainState());

            var result = useCase.ExecuteAsync(true, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("pong", result.message);
            Assert.AreEqual(1, dispatcher.CallCount);
        }

        [Test]
        public void ExecuteAsync_NotVerbose_ReturnsMessage_WithoutDispatch()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var useCase = new PingUseCase(dispatcher, new StubEditorDomainState());

            var result = useCase.ExecuteAsync(false, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("pong", result.message);
            Assert.AreEqual(0, dispatcher.CallCount);
        }

        [Test]
        public void ExecuteAsync_ReturnsDomainState()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var domainState = new StubEditorDomainState { DomainId = "abc", FailedCompilationCount = 2 };
            var useCase = new PingUseCase(dispatcher, domainState);

            // Act
            var result = useCase.ExecuteAsync(false, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual("ok", result.status);
            Assert.AreEqual("abc", result.domainId);
            Assert.AreEqual(2, result.failedCompilationCount);
        }
    }
}
