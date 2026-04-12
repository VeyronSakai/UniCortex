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
    internal sealed class StartProfilerRecordingHandlerTest
    {
        private SpyProfilerOperations _operations;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            _operations = new SpyProfilerOperations();
            var useCase = new StartProfilerRecordingUseCase(dispatcher, _operations);
            var handler = new StartProfilerRecordingHandler(useCase);
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void Handle_Returns200_And_UsesDefaultProfileEditorFalse_WhenBodyEmpty()
        {
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.ProfilerStartRecording);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(1, _operations.StartRecordingCallCount);
            Assert.That(_operations.LastProfileEditor, Is.False);
            var response = JsonUtility.FromJson<StartProfilerRecordingResponse>(context.ResponseBody);
            Assert.That(response.success, Is.True);
        }

        [Test]
        public void Handle_Returns200_And_PassesProfileEditorFlag()
        {
            var body = JsonUtility.ToJson(new StartProfilerRecordingRequest { profileEditor = true });
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.ProfilerStartRecording, body);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(1, _operations.StartRecordingCallCount);
            Assert.That(_operations.LastProfileEditor, Is.True);
        }
    }
}
