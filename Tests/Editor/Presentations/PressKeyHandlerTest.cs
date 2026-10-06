using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Input;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using KeyAction = UniCortex.Editor.Tests.TestDoubles.SpyInputOperations.KeyAction;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class PressKeyHandlerTest
    {
        private static (RequestRouter router, SpyInputOperations ops, FakePlayerLoopDispatcher playerLoopDispatcher)
            CreateRouter()
        {
            var ops = new SpyInputOperations();
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var useCase = new PressKeyUseCase(
                new PlayerLoopRunner(new FakeMainThreadDispatcher(), playerLoopDispatcher), ops,
                playerLoopDispatcher.Time);
            var handler = new PressKeyHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);
            return (router, ops, playerLoopDispatcher);
        }

        private static FakeRequestContext CreateContext(string body)
        {
            return new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputKeyPress, body);
        }

        [Test]
        public void Handle_Returns200_AndPressesAndReleasesKeys()
        {
            // Arrange
            var (router, ops, _) = CreateRouter();
            var context = CreateContext($"{{\"keys\":[\"{KeyName.LeftCtrl}\",\"{KeyName.S}\"]}}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(2, ops.KeyEventHistory.Count);
            Assert.AreEqual(KeyAction.Press, ops.KeyEventHistory[0].Action);
            Assert.AreEqual(KeyAction.Release, ops.KeyEventHistory[1].Action);
            CollectionAssert.AreEqual(new[] { KeyName.LeftCtrl, KeyName.S }, ops.KeyEventHistory[0].Keys);
            StringAssert.Contains("\"success\":true", context.ResponseBody);
        }

        [Test]
        public void Handle_UsesHoldDuration()
        {
            // Arrange
            // Frames are 0.25 seconds apart: press, 3 frames to release after 0.6s, and one frame after the release.
            var (router, _, playerLoopDispatcher) = CreateRouter();
            var context = CreateContext($"{{\"keys\":[\"{KeyName.W}\"],\"holdDuration\":0.6}}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(5, playerLoopDispatcher.FrameCount);
        }

        [Test]
        public void Handle_Returns400_WhenBodyEmpty()
        {
            // Arrange
            var (router, ops, _) = CreateRouter();
            var context = CreateContext("");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains(PressKeyUseCase.KeysRequiredMessage, context.ResponseBody);
            CollectionAssert.IsEmpty(ops.KeyEventHistory);
        }

        [Test]
        public void Handle_Returns400_WhenKeysMissing()
        {
            // Arrange
            var (router, ops, _) = CreateRouter();
            var context = CreateContext("{\"holdDuration\":0.5}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains(PressKeyUseCase.KeysRequiredMessage, context.ResponseBody);
            CollectionAssert.IsEmpty(ops.KeyEventHistory);
        }

        [Test]
        public void Handle_Returns400_WhenHoldDurationIsNegative()
        {
            // Arrange
            var (router, ops, _) = CreateRouter();
            var context = CreateContext($"{{\"keys\":[\"{KeyName.A}\"],\"holdDuration\":-1.0}}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("holdDuration", context.ResponseBody);
            CollectionAssert.IsEmpty(ops.KeyEventHistory);
        }
    }
}
