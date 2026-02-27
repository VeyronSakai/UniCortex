using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.ComponentOps;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class RemoveComponentHandlerTest
    {
        [Test]
        public void HandleRemove_Returns200_WhenValid()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyComponentOperations();
            var useCase = new RemoveComponentUseCase(dispatcher, ops);
            var handler = new RemoveComponentHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext
            {
                HttpMethod = "POST",
                Path = ApiRoutes.ComponentRemove,
                Body = "{\"instanceId\":456,\"componentType\":\"Rigidbody\",\"componentIndex\":0}"
            };

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual(456, ops.LastRemoveComponentInstanceId);
            Assert.AreEqual("Rigidbody", ops.LastRemoveComponentType);
            Assert.AreEqual(0, ops.LastRemoveComponentIndex);
        }

        [Test]
        public void HandleRemove_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyComponentOperations();
            var useCase = new RemoveComponentUseCase(dispatcher, ops);
            var handler = new RemoveComponentHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext
            {
                HttpMethod = "POST",
                Path = ApiRoutes.ComponentRemove,
                Body = ""
            };

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
        }

        [Test]
        public void HandleRemove_Returns400_WhenComponentTypeMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyComponentOperations();
            var useCase = new RemoveComponentUseCase(dispatcher, ops);
            var handler = new RemoveComponentHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext
            {
                HttpMethod = "POST",
                Path = ApiRoutes.ComponentRemove,
                Body = "{\"instanceId\":456}"
            };

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("componentType is required", context.ResponseBody);
        }
    }
}
