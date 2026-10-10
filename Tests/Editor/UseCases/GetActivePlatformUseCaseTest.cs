using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetActivePlatformUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsActiveBuildTarget_And_DispatchesToMainThread()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyBuildTargetOperations { ActiveBuildTarget = "Android" };
            var useCase = new GetActivePlatformUseCase(dispatcher, operations);

            // Act
            var result = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual("Android", result.activeBuildTarget);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
