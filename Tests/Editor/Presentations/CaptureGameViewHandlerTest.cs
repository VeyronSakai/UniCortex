using System;
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
    internal sealed class CaptureGameViewHandlerTest
    {
        private SpyEditorApplication _editorApplication;
        private SpyCaptureOperations _operations;
        private SpyPlayModeViewOperations _playModeViewOperations;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            _editorApplication = new SpyEditorApplication { IsPlaying = true };
            _operations = new SpyCaptureOperations
            {
                GameViewResult = new byte[] { 0x89, 0x50, 0x4E, 0x47 }
            };
            _playModeViewOperations = new SpyPlayModeViewOperations();
            var useCase = new CaptureGameViewUseCase(dispatcher, _editorApplication,
                new SpyEditorWindowOperations(), _operations, _playModeViewOperations);
            var handler = new CaptureGameViewHandler(useCase);
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void HandleCaptureGameView_Returns200_WithPngData()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.GameViewCapture);

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            var response = JsonUtility.FromJson<CaptureGameViewResponse>(context.ResponseBody);
            var pngData = Convert.FromBase64String(response.pngDataBase64);
            Assert.AreEqual(4, pngData.Length);
            Assert.AreEqual(0x89, pngData[0]);
            Assert.AreEqual(1, _operations.CaptureGameViewCallCount);
            Assert.IsNull(_operations.LastSafeAreaToDraw);
        }

        [Test]
        public void HandleCaptureGameView_PassesSafeArea_WhenDrawSafeAreaIsTrue()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.GameViewCapture);
            context.SetQueryParameter(nameof(CaptureGameViewRequest.drawSafeArea), "true");

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.IsNotNull(_operations.LastSafeAreaToDraw);
            Assert.IsFalse(_operations.LastDrawDeviceFrame);
        }

        [Test]
        public void HandleCaptureGameView_DrawsDeviceFrameByDefault_WhenSimulatorView()
        {
            // Arrange
            _playModeViewOperations.ViewType = PlayModeViewTypes.SimulatorView;
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.GameViewCapture);

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.IsTrue(_operations.LastDrawDeviceFrame);
            Assert.IsNull(_operations.LastSafeAreaToDraw);
        }

        [Test]
        public void HandleCaptureGameView_Returns400_WhenGameViewAndDeviceFrameIsTrue()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.GameViewCapture);
            context.SetQueryParameter(nameof(CaptureGameViewRequest.deviceFrame), "true");

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("Simulator view", context.ResponseBody);
            Assert.AreEqual(0, _operations.CaptureGameViewCallCount);
        }

        [Test]
        public void HandleCaptureGameView_DoesNotDrawDeviceFrame_WhenDeviceFrameIsFalse()
        {
            // Arrange
            _playModeViewOperations.ViewType = PlayModeViewTypes.SimulatorView;
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.GameViewCapture);
            context.SetQueryParameter(nameof(CaptureGameViewRequest.deviceFrame), "false");

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.IsFalse(_operations.LastDrawDeviceFrame);
        }

        [Test]
        public void HandleCaptureGameView_Returns400_WhenNotPlaying()
        {
            // Arrange
            _editorApplication.IsPlaying = false;
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.GameViewCapture);

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("Play Mode", context.ResponseBody);
            Assert.AreEqual(0, _operations.CaptureGameViewCallCount);
        }
    }
}
