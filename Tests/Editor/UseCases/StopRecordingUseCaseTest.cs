using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class StopRecordingUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsStopRecording_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyRecordingOperations
            {
                StopRecordingResult = "/tmp/output.mp4"
            };
            var useCase = new StopRecordingUseCase(dispatcher, operations);

            var result = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.StopRecordingCallCount);
            Assert.AreEqual("/tmp/output.mp4", result);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
