using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SetTimelineClipPropertyUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsSetClipProperty_And_DispatchesToMainThread()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new SetTimelineClipPropertyUseCase(dispatcher, ops);

            // Act
            useCase.ExecuteAsync(12345, 1, 2, "postPlayback", "Active", CancellationToken.None)
                .GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, ops.SetClipPropertyCallCount);
            Assert.AreEqual(12345, ops.LastSetClipPropertyInstanceId);
            Assert.AreEqual(1, ops.LastSetClipPropertyTrackIndex);
            Assert.AreEqual(2, ops.LastSetClipPropertyClipIndex);
            Assert.AreEqual("postPlayback", ops.LastSetClipPropertyPath);
            Assert.AreEqual("Active", ops.LastSetClipPropertyValue);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
