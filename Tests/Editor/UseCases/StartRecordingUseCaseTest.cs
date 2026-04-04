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

            useCase.ExecuteAsync(0, 60, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.StartRecordingCallCount);
            Assert.AreEqual(0, operations.LastStartIndex);
            Assert.AreEqual(60, operations.LastFps);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
