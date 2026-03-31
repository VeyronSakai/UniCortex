using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.GameView;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;
using UnityEngine;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class StartGameViewRecordHandlerTest
    {
        private SpyRecordingOperations _operations;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            _operations = new SpyRecordingOperations();
            var useCase = new StartRecordingUseCase(dispatcher, _operations);
            var handler = new StartGameViewRecordHandler(useCase);
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void Handle_Returns200_WithSuccess()
        {
            var body = JsonUtility.ToJson(new StartRecordingRequest { fps = 60, outputPath = "/tmp/test.mp4" });
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.GameViewRecordStart, body);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            var response = JsonUtility.FromJson<StartRecordingResponse>(context.ResponseBody);
            Assert.IsTrue(response.success);
            Assert.AreEqual(1, _operations.StartRecordingCallCount);
            Assert.AreEqual(60, _operations.LastFps);
            Assert.AreEqual("/tmp/test.mp4", _operations.LastOutputPath);
        }

        [Test]
        public void Handle_DefaultsFpsTo30_WhenNotSpecified()
        {
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.GameViewRecordStart);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(30, _operations.LastFps);
        }
    }
}
