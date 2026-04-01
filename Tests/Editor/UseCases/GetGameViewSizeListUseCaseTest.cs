using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetGameViewSizeListUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsSizeList_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyEditorWindowOperations { SelectedSizeIndex = 1 };
            var useCase = new GetGameViewSizeListUseCase(dispatcher, operations);

            var result = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(2, result.sizes.Length);
            Assert.AreEqual(1, result.selectedIndex);
            Assert.AreEqual("Free Aspect", result.sizes[0].name);
            Assert.AreEqual("1920x1080", result.sizes[1].name);
            Assert.AreEqual(1, operations.GetGameViewSizeListCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
