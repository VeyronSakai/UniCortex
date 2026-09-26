using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetTimelineTrackPropertiesUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsProperties_And_DispatchesToMainThread()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new GetTimelineTrackPropertiesUseCase(dispatcher, ops);

            // Act
            var result = useCase.ExecuteAsync(12345, "Assets/Test.playable", 1, CancellationToken.None)
                .GetAwaiter().GetResult();

            // Assert
            Assert.AreSame(ops.GetTrackPropertiesResult, result);
            Assert.AreEqual(1, ops.GetTrackPropertiesCallCount);
            Assert.AreEqual(12345, ops.LastGetTrackPropertiesInstanceId);
            Assert.AreEqual("Assets/Test.playable", ops.LastGetTrackPropertiesAssetPath);
            Assert.AreEqual(1, ops.LastGetTrackPropertiesTrackIndex);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
