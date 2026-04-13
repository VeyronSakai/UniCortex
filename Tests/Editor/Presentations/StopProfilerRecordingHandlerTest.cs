using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Profiler;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class StopProfilerRecordingHandlerTest
    {
        [Test]
        public void Handle_Returns200WithSuccess()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyProfilerOperations();
            var useCase = new StopProfilerRecordingUseCase(dispatcher, operations);
            var handler = new StopProfilerRecordingHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.ProfilerStopRecording);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual(1, operations.StopRecordingCallCount);
        }
    }
}
