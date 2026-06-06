using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetGameViewScaleUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsGameViewScale_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyEditorWindowOperations { Scale = 1.5f, MinScale = 0.5f, MaxScale = 5.0f };
            var useCase = new GetGameViewScaleUseCase(dispatcher, operations);

            var result = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1.5f, result.scale);
            Assert.AreEqual(0.5f, result.minScale);
            Assert.AreEqual(5.0f, result.maxScale);
            Assert.AreEqual(1, operations.GetGameViewScaleCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
