using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetProfilerStatusUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsProfilerStatus_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyProfilerOperations
            {
                StatusResponse = new GetProfilerStatusResponse(true, true, true, true)
            };
            var useCase = new GetProfilerStatusUseCase(dispatcher, operations);

            var result = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(result.isWindowOpen, Is.True);
            Assert.That(result.hasFocus, Is.True);
            Assert.That(result.isRecording, Is.True);
            Assert.That(result.profileEditor, Is.True);
            Assert.AreEqual(1, operations.GetStatusCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
