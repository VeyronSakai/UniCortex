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
    internal sealed class StopMovieRecorderHandlerTest
    {
        private SpyMovieRecordingOperations _operations;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            _operations = new SpyMovieRecordingOperations
            {
                StopMovieRecordingResult = "/tmp/output.mp4"
            };
            var useCase = new StopMovieRecordingUseCase(dispatcher, _operations);
            var handler = new StopMovieRecorderHandler(useCase);
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void Handle_Returns200_WithOutputPath()
        {
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.RecorderMovieStop);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            var response = JsonUtility.FromJson<StopMovieRecordingResponse>(context.ResponseBody);
            Assert.AreEqual("/tmp/output.mp4", response.outputPath);
            Assert.AreEqual(1, _operations.StopMovieRecordingCallCount);
        }
    }
}
