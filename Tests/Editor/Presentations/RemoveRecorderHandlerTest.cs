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
    internal sealed class RemoveRecorderHandlerTest
    {
        private SpyRecordingOperations _operations;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            _operations = new SpyRecordingOperations();
            _operations.AddRecorder("Movie", "/tmp/out.mp4", string.Empty, string.Empty);
            var useCase = new RemoveRecorderUseCase(dispatcher, _operations);
            var handler = new RemoveRecorderHandler(useCase);
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void Handle_Returns200_WithSuccess()
        {
            var body = JsonUtility.ToJson(new RemoveRecorderRequest { index = 0 });
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.RecorderRemove, body);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(1, _operations.RemoveCallCount);
            Assert.AreEqual(0, _operations.LastRemoveIndex);
        }

        [Test]
        public void Handle_Returns400_WhenBodyEmpty()
        {
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.RecorderRemove, "");

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
        }
    }
}
