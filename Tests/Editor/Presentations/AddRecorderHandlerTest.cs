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
    internal sealed class AddRecorderHandlerTest
    {
        private SpyRecordingOperations _operations;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            _operations = new SpyRecordingOperations();
            var useCase = new AddRecorderUseCase(dispatcher, _operations);
            var handler = new AddRecorderHandler(useCase);
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void Handle_Returns200_WithName()
        {
            var body = JsonUtility.ToJson(new AddRecorderRequest
            {
                name = "TestRecorder",
                outputPath = "/tmp/out.mp4"
            });
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.RecorderAdd, body);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(1, _operations.AddCallCount);
            Assert.AreEqual("TestRecorder", _operations.LastAddName);
            Assert.AreEqual("/tmp/out.mp4", _operations.LastAddOutputPath);
            var response = JsonUtility.FromJson<AddRecorderResponse>(context.ResponseBody);
            Assert.AreEqual("TestRecorder", response.name);
        }

        [Test]
        public void Handle_Returns400_WhenNameMissing()
        {
            var body = JsonUtility.ToJson(new AddRecorderRequest { outputPath = "/tmp/out.mp4" });
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.RecorderAdd, body);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
        }

        [Test]
        public void Handle_Returns400_WhenOutputPathMissing()
        {
            var body = JsonUtility.ToJson(new AddRecorderRequest { name = "TestRecorder" });
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.RecorderAdd, body);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
        }
    }
}
