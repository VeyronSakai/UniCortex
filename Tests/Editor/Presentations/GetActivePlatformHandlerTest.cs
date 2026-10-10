using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Editor;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class GetActivePlatformHandlerTest
    {
        [Test]
        public void HandleGetActivePlatform_Returns200WithActiveBuildTarget()
        {
            // Arrange
            var operations = new SpyBuildTargetOperations { ActiveBuildTarget = "Android" };
            var useCase = new GetActivePlatformUseCase(new FakeMainThreadDispatcher(), operations);
            var handler = new GetActivePlatformHandler(useCase);
            var router = new RequestRouter();
            handler.Register(router);
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.Platform);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("\"activeBuildTarget\":\"Android\"", context.ResponseBody);
        }
    }
}
