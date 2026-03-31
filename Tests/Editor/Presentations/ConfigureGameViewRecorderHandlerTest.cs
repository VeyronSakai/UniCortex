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
    internal sealed class ConfigureGameViewRecorderHandlerTest
    {
        private SpyRecordingOperations _operations;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            _operations = new SpyRecordingOperations();
            var useCase = new ConfigureRecorderUseCase(dispatcher, _operations);
            var handler = new ConfigureGameViewRecorderHandler(useCase);
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void Handle_Returns200_WithSuccess()
        {
            var body = JsonUtility.ToJson(new ConfigureRecorderRequest
            {
                outputPath = "/tmp/out.mp4",
                source = "Camera",
                cameraSource = "MainCamera",
                outputFormat = "MP4"
            });
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.GameViewRecorderSettings, body);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(1, _operations.ConfigureCallCount);
            Assert.AreEqual("/tmp/out.mp4", _operations.LastConfigOutputPath);
            Assert.AreEqual("Camera", _operations.LastConfigSource);
        }
    }
}
