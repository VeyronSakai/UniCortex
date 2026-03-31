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
    internal sealed class GetGameViewRecorderSettingsHandlerTest
    {
        private SpyRecordingOperations _operations;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            _operations = new SpyRecordingOperations
            {
                SettingsResult = new GetRecorderSettingsResponse(
                    "/tmp/out.mp4", "GameView", "", "", false, 1920, 1080, "MP4")
            };
            var useCase = new GetRecorderSettingsUseCase(dispatcher, _operations);
            var handler = new GetGameViewRecorderSettingsHandler(useCase);
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void Handle_Returns200_WithSettings()
        {
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.GameViewRecorderSettings);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            var response = JsonUtility.FromJson<GetRecorderSettingsResponse>(context.ResponseBody);
            Assert.AreEqual("/tmp/out.mp4", response.outputPath);
            Assert.AreEqual("GameView", response.source);
            Assert.AreEqual(1920, response.outputWidth);
            Assert.AreEqual(1080, response.outputHeight);
            Assert.AreEqual("MP4", response.outputFormat);
        }
    }
}
