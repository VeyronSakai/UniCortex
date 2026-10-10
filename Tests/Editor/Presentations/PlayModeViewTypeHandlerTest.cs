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
    internal sealed class PlayModeViewTypeHandlerTest
    {
        private SpyPlayModeViewOperations _operations;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            _operations = new SpyPlayModeViewOperations();
            _router = new RequestRouter();
            new GetPlayModeViewTypeHandler(new GetPlayModeViewTypeUseCase(dispatcher, _operations))
                .Register(_router);
            new SetPlayModeViewTypeHandler(new SetPlayModeViewTypeUseCase(dispatcher, _operations))
                .Register(_router);
        }

        [Test]
        public void HandleGet_Returns200_WithViewType()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.GameViewViewType);

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            var response = JsonUtility.FromJson<GetPlayModeViewTypeResponse>(context.ResponseBody);
            Assert.AreEqual(PlayModeViewTypes.GameView, response.viewType);
        }

        [Test]
        public void HandleSet_Returns200_AndSwitchesViewType()
        {
            // Arrange
            var body = JsonUtility.ToJson(new SetPlayModeViewTypeRequest
            {
                viewType = PlayModeViewTypes.SimulatorView
            });
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.GameViewViewType, body);

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            var response = JsonUtility.FromJson<SetPlayModeViewTypeResponse>(context.ResponseBody);
            Assert.AreEqual(PlayModeViewTypes.SimulatorView, response.viewType);
            Assert.AreEqual(PlayModeViewTypes.SimulatorView, _operations.ViewType);
        }

        [Test]
        public void HandleSet_Returns400_WhenViewTypeIsMissing()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.GameViewViewType, "{}");

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("viewType is required", context.ResponseBody);
            Assert.AreEqual(0, _operations.SetViewTypeCallCount);
        }

        [Test]
        public void HandleSet_Returns400_WhenViewTypeIsUnknown()
        {
            // Arrange
            var body = JsonUtility.ToJson(new SetPlayModeViewTypeRequest { viewType = "Unknown" });
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.GameViewViewType, body);

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("Unknown view type", context.ResponseBody);
            Assert.AreEqual(0, _operations.SetViewTypeCallCount);
        }
    }
}
