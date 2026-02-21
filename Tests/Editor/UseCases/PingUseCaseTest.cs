using EditorBridge.Editor.Tests.TestDoubles;
using EditorBridge.Editor.UseCases;
using NUnit.Framework;

namespace EditorBridge.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class PingUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsMessage_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var useCase = new PingUseCase(dispatcher);

            var result = useCase.ExecuteAsync().GetAwaiter().GetResult();

            Assert.AreEqual("pong", result);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
