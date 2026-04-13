using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Profiler;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;
using UnityEngine;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class GetProfilerStatusHandlerTest
    {
        [Test]
        public void Handle_Returns200WithProfilerStatus()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyProfilerOperations
            {
                StatusResponse = new GetProfilerStatusResponse(true, false, true, true)
            };
            var useCase = new GetProfilerStatusUseCase(dispatcher, operations);
            var handler = new GetProfilerStatusHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.ProfilerStatus);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            var response = JsonUtility.FromJson<GetProfilerStatusResponse>(context.ResponseBody);
            Assert.That(response.isWindowOpen, Is.True);
            Assert.That(response.hasFocus, Is.False);
            Assert.That(response.isRecording, Is.True);
            Assert.That(response.profileEditor, Is.True);
            Assert.AreEqual(1, operations.GetStatusCallCount);
        }
    }
}
