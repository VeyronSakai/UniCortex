using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetRecorderListUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsList_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyRecordingOperations();
            operations.AddRecorder("Movie", "/tmp/out.mp4", string.Empty, string.Empty);
            var useCase = new GetRecorderListUseCase(dispatcher, operations);

            var result = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("Movie", result[0].name);
            Assert.AreEqual("/tmp/out.mp4", result[0].outputPath);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
