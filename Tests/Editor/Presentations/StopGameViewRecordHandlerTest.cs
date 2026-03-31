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
    internal sealed class StopGameViewRecordHandlerTest
    {
        private SpyRecordingOperations _operations;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            _operations = new SpyRecordingOperations
            {
                StopRecordingResult = "/tmp/output.mp4"
            };
            var useCase = new StopRecordingUseCase(dispatcher, _operations);
            var handler = new StopGameViewRecordHandler(useCase);
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void Handle_Returns200_WithOutputPath()
        {
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.GameViewRecorderStop);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            var response = JsonUtility.FromJson<StopRecordingResponse>(context.ResponseBody);
            Assert.AreEqual("/tmp/output.mp4", response.outputPath);
            Assert.AreEqual(1, _operations.StopRecordingCallCount);
        }
    }
}
