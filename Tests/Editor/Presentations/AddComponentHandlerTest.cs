using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;
using UniCortex.Editor.Handlers.Component;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class AddComponentHandlerTest
    {
        [Test]
        public void HandleAdd_Returns200_WhenValid()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyComponentOperations();
            var useCase = new AddComponentUseCase(dispatcher, ops);
            var handler = new AddComponentHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.ComponentAdd,
                "{\"instanceId\":123,\"componentType\":\"UnityEngine.Rigidbody\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual(123, ops.LastAddComponentInstanceId);
            Assert.AreEqual("UnityEngine.Rigidbody", ops.LastAddComponentType);
        }

        [Test]
        public void HandleAdd_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyComponentOperations();
            var useCase = new AddComponentUseCase(dispatcher, ops);
            var handler = new AddComponentHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.ComponentAdd, "");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
        }

        [Test]
        public void HandleAdd_Returns400_WhenComponentTypeMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyComponentOperations();
            var useCase = new AddComponentUseCase(dispatcher, ops);
            var handler = new AddComponentHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.ComponentAdd,
                "{\"instanceId\":123}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("componentType is required", context.ResponseBody);
        }
    }
}
