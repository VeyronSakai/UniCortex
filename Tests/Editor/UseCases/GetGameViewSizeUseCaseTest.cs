using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetGameViewSizeUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsGameViewSize_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyEditorWindowOperations { GameViewWidth = 1920, GameViewHeight = 1080 };
            var useCase = new GetGameViewSizeUseCase(dispatcher, operations);

            var result = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1920, result.screenWidth);
            Assert.AreEqual(1080, result.screenHeight);
            Assert.AreEqual(1, operations.GetGameViewSizeCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
