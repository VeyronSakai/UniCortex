using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SetTimelineTrackPropertyUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsSetTrackProperty_And_DispatchesToMainThread()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new SetTimelineTrackPropertyUseCase(dispatcher, ops);

            // Act
            useCase.ExecuteAsync(12345, 1, "m_Muted", "true", CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, ops.SetTrackPropertyCallCount);
            Assert.AreEqual(12345, ops.LastSetTrackPropertyInstanceId);
            Assert.AreEqual(1, ops.LastSetTrackPropertyTrackIndex);
            Assert.AreEqual("m_Muted", ops.LastSetTrackPropertyPath);
            Assert.AreEqual("true", ops.LastSetTrackPropertyValue);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
