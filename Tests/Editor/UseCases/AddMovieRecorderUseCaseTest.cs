using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class AddMovieRecorderUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsAddRecorder_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyMovieRecordingOperations();
            var useCase = new AddMovieRecorderUseCase(dispatcher, operations);

            var name = useCase.ExecuteAsync("TestRecorder", "/tmp/out.mp4", string.Empty, string.Empty,
                false, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("TestRecorder", name);
            Assert.AreEqual(1, operations.AddCallCount);
            Assert.AreEqual("TestRecorder", operations.LastAddName);
            Assert.AreEqual("/tmp/out.mp4", operations.LastAddOutputPath);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
