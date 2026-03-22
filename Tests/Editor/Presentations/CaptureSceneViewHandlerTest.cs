using System;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.SceneView;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;
using UnityEngine;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class CaptureSceneViewHandlerTest
    {
        private SpyCaptureOperations _operations;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            _operations = new SpyCaptureOperations
            {
                ScreenshotResult = new byte[] { 0x89, 0x50, 0x4E, 0x47 }
            };
            var useCase = new CaptureSceneViewUseCase(dispatcher, _operations);
            var handler = new CaptureSceneViewHandler(useCase);
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void HandleCaptureSceneView_Returns200_WithPngData()
        {
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.SceneViewCapture);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            var response = JsonUtility.FromJson<CaptureSceneViewResponse>(context.ResponseBody);
            var pngData = Convert.FromBase64String(response.pngDataBase64);
            Assert.AreEqual(4, pngData.Length);
            Assert.AreEqual(0x89, pngData[0]);
            Assert.AreEqual(1, _operations.CaptureSceneViewCallCount);
        }
    }
}
