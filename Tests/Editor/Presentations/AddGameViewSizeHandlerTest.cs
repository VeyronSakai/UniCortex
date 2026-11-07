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
    internal sealed class AddGameViewSizeHandlerTest
    {
        private SpyEditorWindowOperations _operations;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            _operations = new SpyEditorWindowOperations();
            var useCase = new AddGameViewSizeUseCase(dispatcher, _operations);
            var handler = new AddGameViewSizeHandler(useCase);

            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void HandleAddGameViewSize_Returns200_WithIndexOfAddedSize()
        {
            // Arrange
            var body = JsonUtility.ToJson(new AddGameViewSizeRequest { name = "Phone Half", width = 642, height = 1389 });
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.GameViewSizeAdd, body);

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            var response = JsonUtility.FromJson<AddGameViewSizeResponse>(context.ResponseBody);
            Assert.AreEqual(2, response.index);
            Assert.IsTrue(response.added);
            Assert.AreEqual(("Phone Half", 642, 1389), _operations.LastAddedSize);
        }

        [Test]
        public void HandleAddGameViewSize_Returns400_WhenSizeIsOutOfRange()
        {
            // Arrange
            var body = JsonUtility.ToJson(new AddGameViewSizeRequest { name = "Phone Half", width = 5, height = 1389 });
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.GameViewSizeAdd, body);

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("Width", context.ResponseBody);
            Assert.AreEqual(0, _operations.AddGameViewSizeCallCount);
        }

        [Test]
        public void HandleAddGameViewSize_Returns400_WhenBodyIsEmpty()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.GameViewSizeAdd, "");

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, _operations.AddGameViewSizeCallCount);
        }
    }
}
