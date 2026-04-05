using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.MovieRecorder;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;
using UnityEngine;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class StartMovieRecorderHandlerTest
    {
        private SpyMovieRecordingOperations _operations;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            _operations = new SpyMovieRecordingOperations();
            var useCase = new StartMovieRecordingUseCase(dispatcher, _operations);
            var handler = new StartMovieRecorderHandler(useCase);
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void Handle_Returns200_WithSuccess()
        {
            var body = JsonUtility.ToJson(new StartMovieRecordingRequest
            {
                index = 0,
                fps = 60
            });
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.MovieRecorderStart, body);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(1, _operations.StartMovieRecordingCallCount);
            Assert.AreEqual(0, _operations.LastStartIndex);
            Assert.AreEqual(60, _operations.LastFps);
        }

        [Test]
        public void Handle_DefaultsFpsTo30_WhenNotSpecified()
        {
            var body = JsonUtility.ToJson(new StartMovieRecordingRequest { index = 0 });
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.MovieRecorderStart, body);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(30, _operations.LastFps);
        }
    }
}
