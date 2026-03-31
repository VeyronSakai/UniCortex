using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class StartRecordingUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsStartRecording_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyRecordingOperations();
            var useCase = new StartRecordingUseCase(dispatcher, operations);

            useCase.ExecuteAsync(60, "Variable", "TimeInterval",
                1.0f, 5.0f, 0, 0, 0, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.StartRecordingCallCount);
            Assert.AreEqual(60, operations.LastFps);
            Assert.AreEqual("Variable", operations.LastFrameRatePlayback);
            Assert.AreEqual("TimeInterval", operations.LastRecordMode);
            Assert.AreEqual(1.0f, operations.LastStartTime);
            Assert.AreEqual(5.0f, operations.LastEndTime);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
