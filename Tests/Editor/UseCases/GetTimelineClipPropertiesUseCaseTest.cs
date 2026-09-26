using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetTimelineClipPropertiesUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsProperties_And_DispatchesToMainThread()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new GetTimelineClipPropertiesUseCase(dispatcher, ops);

            // Act
            var result = useCase.ExecuteAsync(12345, "Assets/Test.playable", 1, 2, CancellationToken.None)
                .GetAwaiter().GetResult();

            // Assert
            Assert.AreSame(ops.GetClipPropertiesResult, result);
            Assert.AreEqual(1, ops.GetClipPropertiesCallCount);
            Assert.AreEqual(12345, ops.LastGetClipPropertiesInstanceId);
            Assert.AreEqual("Assets/Test.playable", ops.LastGetClipPropertiesAssetPath);
            Assert.AreEqual(1, ops.LastGetClipPropertiesTrackIndex);
            Assert.AreEqual(2, ops.LastGetClipPropertiesClipIndex);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
