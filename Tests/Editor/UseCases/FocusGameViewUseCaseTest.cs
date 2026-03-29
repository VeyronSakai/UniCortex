using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class FocusGameViewUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsFocusGameView_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyEditorWindowOperations();
            var useCase = new FocusGameViewUseCase(dispatcher, operations);

            useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.FocusGameViewCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
