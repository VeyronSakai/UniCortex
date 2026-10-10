using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetScreenSafeAreaUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsSafeArea_And_DispatchesToMainThread()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPlayModeViewOperations();
            var useCase = new GetScreenSafeAreaUseCase(dispatcher, operations);

            // Act
            var result = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1170, result.screenWidth);
            Assert.AreEqual(102f, result.safeArea.y);
            Assert.AreEqual(1, result.cutouts.Length);
            Assert.AreEqual(1, operations.GetScreenSafeAreaCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
