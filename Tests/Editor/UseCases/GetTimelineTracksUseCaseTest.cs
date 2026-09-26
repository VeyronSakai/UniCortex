using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetTimelineTracksUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsTimeline_And_DispatchesToMainThread()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new GetTimelineTracksUseCase(dispatcher, ops);

            // Act
            var result = useCase.ExecuteAsync(12345, "Assets/Test.playable", CancellationToken.None)
                .GetAwaiter().GetResult();

            // Assert
            Assert.AreSame(ops.GetTracksResult, result);
            Assert.AreEqual(1, ops.GetTracksCallCount);
            Assert.AreEqual(12345, ops.LastGetTracksInstanceId);
            Assert.AreEqual("Assets/Test.playable", ops.LastGetTracksAssetPath);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
