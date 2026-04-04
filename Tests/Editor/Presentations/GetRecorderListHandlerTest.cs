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
    internal sealed class GetRecorderListHandlerTest
    {
        private SpyRecordingOperations _operations;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            _operations = new SpyRecordingOperations();
            _operations.AddRecorder("Movie", "/tmp/out.mp4", "", "");
            var useCase = new GetRecorderListUseCase(dispatcher, _operations);
            var handler = new GetRecorderListHandler(useCase);
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void Handle_Returns200_WithRecorderList()
        {
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.RecorderList);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            var response = JsonUtility.FromJson<GetRecorderListResponse>(context.ResponseBody);
            Assert.AreEqual(1, response.recorders.Length);
            Assert.AreEqual("Movie", response.recorders[0].name);
            Assert.AreEqual("/tmp/out.mp4", response.recorders[0].outputPath);
        }
    }
}
