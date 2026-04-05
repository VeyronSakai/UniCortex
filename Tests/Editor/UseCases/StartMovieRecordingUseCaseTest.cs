using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class StartMovieRecordingUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsStartRecording_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyMovieRecordingOperations();
            var useCase = new StartMovieRecordingUseCase(dispatcher, operations);

            useCase.ExecuteAsync(0, 60, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.StartMovieRecordingCallCount);
            Assert.AreEqual(0, operations.LastStartIndex);
            Assert.AreEqual(60, operations.LastFps);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
