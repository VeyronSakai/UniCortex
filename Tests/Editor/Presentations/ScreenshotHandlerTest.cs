using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Utility;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class ScreenshotHandlerTest
    {
        [Test]
        public void HandleScreenshot_Returns200_WithPngData()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyUtilityOperations
            {
                ScreenshotResult = new byte[] { 0x89, 0x50, 0x4E, 0x47 }
            };
            var useCase = new CaptureScreenshotUseCase(dispatcher, operations);
            var handler = new ScreenshotHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext
            {
                HttpMethod = "GET",
                Path = ApiRoutes.Screenshot
            };

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            Assert.AreEqual("image/png", context.ResponseContentType);
            Assert.AreEqual(4, context.ResponseBinaryData.Length);
            Assert.AreEqual(0x89, context.ResponseBinaryData[0]);
        }
    }
}
