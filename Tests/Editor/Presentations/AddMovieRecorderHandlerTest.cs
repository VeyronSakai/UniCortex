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
    internal sealed class AddMovieRecorderHandlerTest
    {
        private SpyMovieRecordingOperations _operations;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            _operations = new SpyMovieRecordingOperations();
            var useCase = new AddMovieRecorderUseCase(dispatcher, _operations);
            var handler = new AddMovieRecorderHandler(useCase);
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void Handle_Returns200_WithName()
        {
            var body = JsonUtility.ToJson(new AddMovieRecorderRequest
            {
                name = "TestRecorder",
                outputPath = "/tmp/out.mp4"
            });
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.MovieRecorderAdd, body);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(1, _operations.AddCallCount);
            Assert.AreEqual("TestRecorder", _operations.LastAddName);
            Assert.AreEqual("/tmp/out.mp4", _operations.LastAddOutputPath);
            var response = JsonUtility.FromJson<AddMovieRecorderResponse>(context.ResponseBody);
            Assert.AreEqual("TestRecorder", response.name);
        }

        [Test]
        public void Handle_Returns400_WhenNameMissing()
        {
            var body = JsonUtility.ToJson(new AddMovieRecorderRequest { outputPath = "/tmp/out.mp4" });
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.MovieRecorderAdd, body);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
        }

        [Test]
        public void Handle_Returns400_WhenOutputPathMissing()
        {
            var body = JsonUtility.ToJson(new AddMovieRecorderRequest { name = "TestRecorder" });
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.MovieRecorderAdd, body);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
        }
    }
}
