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
    internal sealed class GetScreenSafeAreaHandlerTest
    {
        [Test]
        public void HandleGetSafeArea_Returns200_WithSafeAreaAndCutouts()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPlayModeViewOperations();
            var router = new RequestRouter();
            new GetScreenSafeAreaHandler(new GetScreenSafeAreaUseCase(dispatcher, operations)).Register(router);
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.GameViewSafeArea);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            var response = JsonUtility.FromJson<GetScreenSafeAreaResponse>(context.ResponseBody);
            Assert.AreEqual(PlayModeViewTypes.SimulatorView, response.viewType);
            Assert.AreEqual("Portrait", response.orientation);
            Assert.AreEqual(2532, response.screenHeight);
            Assert.AreEqual(2289f, response.safeArea.height);
            Assert.AreEqual(1, response.cutouts.Length);
            Assert.AreEqual(312f, response.cutouts[0].x);
        }
    }
}
