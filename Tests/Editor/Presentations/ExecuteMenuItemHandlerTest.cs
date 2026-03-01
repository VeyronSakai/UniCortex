using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.MenuItem;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class ExecuteMenuItemHandlerTest
    {
        [Test]
        public void HandleExecute_Returns200_WhenValid()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyMenuItemOperations();
            var useCase = new ExecuteMenuItemUseCase(dispatcher, operations);
            var handler = new ExecuteMenuItemHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.MenuItemExecute, "{\"menuPath\":\"GameObject/3D Object/Cube\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual("GameObject/3D Object/Cube", operations.LastMenuPath);
        }

        [Test]
        public void HandleExecute_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyMenuItemOperations();
            var useCase = new ExecuteMenuItemUseCase(dispatcher, operations);
            var handler = new ExecuteMenuItemHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.MenuItemExecute, "");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("menuPath is required", context.ResponseBody);
        }

        [Test]
        public void HandleExecute_Returns400_WhenMenuPathMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyMenuItemOperations();
            var useCase = new ExecuteMenuItemUseCase(dispatcher, operations);
            var handler = new ExecuteMenuItemHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.MenuItemExecute, "{}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("menuPath is required", context.ResponseBody);
        }

        [Test]
        public void HandleExecute_Returns404_WhenMenuItemNotFound()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyMenuItemOperations { ExecuteMenuItemResult = false };
            var useCase = new ExecuteMenuItemUseCase(dispatcher, operations);
            var handler = new ExecuteMenuItemHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.MenuItemExecute, "{\"menuPath\":\"Invalid/Menu/Path\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(404, context.ResponseStatusCode);
            StringAssert.Contains("Failed to execute menu item", context.ResponseBody);
        }
    }
}
