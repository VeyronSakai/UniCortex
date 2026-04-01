using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SetGameViewSizeUseCaseTest
    {
        [Test]
        public void ExecuteAsync_WithIndex_SetsGameViewSizeByIndex()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyEditorWindowOperations();
            var useCase = new SetGameViewSizeUseCase(dispatcher, operations);
            var request = new SetGameViewSizeRequest { index = 3 };

            var result = useCase.ExecuteAsync(request, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(true, result.success);
            Assert.AreEqual(3, operations.LastSetIndex);
            Assert.AreEqual(1, operations.SetGameViewSizeByIndexCallCount);
            Assert.AreEqual(0, operations.SetGameViewSizeCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }

        [Test]
        public void ExecuteAsync_WithWidthAndHeight_SetsGameViewSize()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyEditorWindowOperations();
            var useCase = new SetGameViewSizeUseCase(dispatcher, operations);
            var request = new SetGameViewSizeRequest { width = 1920, height = 1080 };

            var result = useCase.ExecuteAsync(request, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(true, result.success);
            Assert.AreEqual(1920, operations.GameViewWidth);
            Assert.AreEqual(1080, operations.GameViewHeight);
            Assert.AreEqual(0, operations.SetGameViewSizeByIndexCallCount);
            Assert.AreEqual(1, operations.SetGameViewSizeCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
