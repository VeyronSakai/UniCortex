using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SetGameViewSizeUseCaseTest
    {
        [Test]
        public void ExecuteAsync_SetsGameViewSizeByIndex_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyEditorWindowOperations();
            var useCase = new SetGameViewSizeUseCase(dispatcher, operations);

            var result = useCase.ExecuteAsync(3, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(true, result.success);
            Assert.AreEqual(3, operations.LastSetIndex);
            Assert.AreEqual(1, operations.SetGameViewSizeCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
