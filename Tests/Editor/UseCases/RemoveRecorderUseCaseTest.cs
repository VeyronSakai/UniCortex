using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class RemoveRecorderUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsRemoveRecorder_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyRecordingOperations();
            operations.AddRecorder("Movie", "/tmp/out.mp4", "", "");
            var useCase = new RemoveRecorderUseCase(dispatcher, operations);

            useCase.ExecuteAsync(0, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.RemoveCallCount);
            Assert.AreEqual(0, operations.LastRemoveIndex);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
