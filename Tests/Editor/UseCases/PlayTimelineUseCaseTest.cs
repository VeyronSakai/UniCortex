using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class PlayTimelineUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsPlay_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new PlayTimelineUseCase(dispatcher, ops);

            useCase.ExecuteAsync(12345, CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.PlayCallCount);
            Assert.AreEqual(12345, ops.LastPlayInstanceId);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
