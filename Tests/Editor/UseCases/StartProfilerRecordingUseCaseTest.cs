using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class StartProfilerRecordingUseCaseTest
    {
        [Test]
        public void ExecuteAsync_StartsRecordingWithProfileEditorFlag_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyProfilerOperations();
            var useCase = new StartProfilerRecordingUseCase(dispatcher, operations);

            useCase.ExecuteAsync(true, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.StartRecordingCallCount);
            Assert.That(operations.LastProfileEditor, Is.True);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
