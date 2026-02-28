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
    internal sealed class SetComponentPropertyHandlerTest
    {
        [Test]
        public void HandleSetProperty_Returns200_WhenValid()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyComponentOperations();
            var useCase = new SetComponentPropertyUseCase(dispatcher, ops);
            var handler = new SetComponentPropertyHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.ComponentSetProperty,
                "{\"instanceId\":123,\"componentType\":\"Transform\",\"propertyPath\":\"m_LocalPosition.x\",\"value\":\"1.5\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual(123, ops.LastSetPropertyInstanceId);
            Assert.AreEqual("Transform", ops.LastSetPropertyComponentType);
            Assert.AreEqual("m_LocalPosition.x", ops.LastSetPropertyPath);
            Assert.AreEqual("1.5", ops.LastSetPropertyValue);
        }

        [Test]
        public void HandleSetProperty_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyComponentOperations();
            var useCase = new SetComponentPropertyUseCase(dispatcher, ops);
            var handler = new SetComponentPropertyHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.ComponentSetProperty, "");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
        }

        [Test]
        public void HandleSetProperty_Returns400_WhenPropertyPathMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyComponentOperations();
            var useCase = new SetComponentPropertyUseCase(dispatcher, ops);
            var handler = new SetComponentPropertyHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.ComponentSetProperty,
                "{\"instanceId\":123,\"componentType\":\"Transform\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("propertyPath is required", context.ResponseBody);
        }
    }
}
