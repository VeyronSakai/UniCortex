using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Recorder;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;
using UnityEngine;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class StartRecorderHandlerTest
    {
        private SpyRecordingOperations _operations;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            _operations = new SpyRecordingOperations();
            var useCase = new StartRecordingUseCase(dispatcher, _operations);
            var handler = new StartRecorderHandler(useCase);
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void Handle_Returns200_WithSuccess()
        {
            var body = JsonUtility.ToJson(new StartRecordingRequest
            {
                index = 0,
                fps = 60
            });
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.RecorderStart, body);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(1, _operations.StartRecordingCallCount);
            Assert.AreEqual(0, _operations.LastStartIndex);
            Assert.AreEqual(60, _operations.LastFps);
        }

        [Test]
        public void Handle_DefaultsFpsTo30_WhenNotSpecified()
        {
            var body = JsonUtility.ToJson(new StartRecordingRequest { index = 0 });
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.RecorderStart, body);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(30, _operations.LastFps);
        }
    }
}
