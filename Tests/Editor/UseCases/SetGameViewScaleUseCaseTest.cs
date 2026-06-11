using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SetGameViewScaleUseCaseTest
    {
        [Test]
        public void ExecuteAsync_AppliesClampedScale_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyEditorWindowOperations { MinScale = 0.5f, MaxScale = 5.0f };
            var useCase = new SetGameViewScaleUseCase(dispatcher, operations);

            // Request a value above the max so we can verify it is clamped and reported back.
            var result = useCase.ExecuteAsync(10.0f, CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsTrue(result.success);
            Assert.AreEqual(5.0f, result.scale);
            Assert.AreEqual(5.0f, operations.LastSetScale);
            Assert.AreEqual(1, operations.SetGameViewScaleCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
