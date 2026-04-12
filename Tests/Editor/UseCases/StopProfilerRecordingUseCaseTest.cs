using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class StopProfilerRecordingUseCaseTest
    {
        [Test]
        public void ExecuteAsync_StopsRecording_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyProfilerOperations();
            var useCase = new StopProfilerRecordingUseCase(dispatcher, operations);

            useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.StopRecordingCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
