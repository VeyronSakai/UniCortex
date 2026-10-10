using System.Linq;
using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Input;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class TypeTextHandlerTest
    {
        private static (RequestRouter router, SpyInputOperations ops) CreateRouter()
        {
            var ops = new SpyInputOperations();
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var useCase = new TypeTextUseCase(
                new PlayerLoopRunner(new FakeMainThreadDispatcher(), playerLoopDispatcher), ops);
            var handler = new TypeTextHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);
            return (router, ops);
        }

        private static FakeRequestContext CreateContext(string body)
        {
            return new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputTextType, body);
        }

        [Test]
        public void Handle_Returns200_AndTypesText()
        {
            // Arrange
            var (router, ops) = CreateRouter();
            var context = CreateContext("{\"text\":\"Hello あ\"}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual("Hello あ", ops.TextEventHistory.Single().Text);
            StringAssert.Contains("\"success\":true", context.ResponseBody);
        }

        [Test]
        public void Handle_Returns400_WhenBodyEmpty()
        {
            // Arrange
            var (router, ops) = CreateRouter();
            var context = CreateContext("");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains(TypeTextUseCase.TextRequiredMessage, context.ResponseBody);
            CollectionAssert.IsEmpty(ops.TextEventHistory);
        }

        [Test]
        public void Handle_Returns400_WhenTextMissing()
        {
            // Arrange
            var (router, ops) = CreateRouter();
            var context = CreateContext("{}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains(TypeTextUseCase.TextRequiredMessage, context.ResponseBody);
            CollectionAssert.IsEmpty(ops.TextEventHistory);
        }
    }
}
