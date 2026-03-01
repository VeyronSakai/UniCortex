using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Screenshot;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class ScreenshotHandlerTest
    {
        private SpyScreenshotOperations _operations;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            _operations = new SpyScreenshotOperations
            {
                ScreenshotResult = new byte[] { 0x89, 0x50, 0x4E, 0x47 }
            };
            var useCase = new CaptureScreenshotUseCase(dispatcher, _operations);
            var handler = new ScreenshotHandler(useCase);
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void HandleScreenshot_Returns200_WithPngData()
        {
            var context = new FakeRequestContext("GET", ApiRoutes.ScreenshotCapture);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            Assert.AreEqual("image/png", context.ResponseContentType);
            Assert.AreEqual(4, context.ResponseBinaryData.Length);
            Assert.AreEqual(0x89, context.ResponseBinaryData[0]);
        }
    }
}
