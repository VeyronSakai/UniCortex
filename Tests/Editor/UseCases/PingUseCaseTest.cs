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
            var useCase = new PingUseCase(dispatcher);

            var result = useCase.ExecuteAsync(true, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("pong", result);
            Assert.AreEqual(1, dispatcher.CallCount);
        }

        [Test]
        public void ExecuteAsync_NotVerbose_ReturnsMessage_WithoutDispatch()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var useCase = new PingUseCase(dispatcher);

            var result = useCase.ExecuteAsync(false, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("pong", result);
            Assert.AreEqual(0, dispatcher.CallCount);
        }
    }
}
