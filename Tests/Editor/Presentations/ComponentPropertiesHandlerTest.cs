using System.Collections.Generic;
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
    internal sealed class ComponentPropertiesHandlerTest
    {
        [Test]
        public void HandleProperties_Returns200_WithProperties()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyComponentOperations();
            ops.GetPropertiesResult = new ComponentPropertiesResponse("Transform",
                new List<SerializedPropertyEntry>
                {
                    new SerializedPropertyEntry("m_LocalPosition", "Vector3", "(0, 0, 0)")
                });
            var useCase = new GetComponentPropertiesUseCase(dispatcher, ops);
            var handler = new ComponentPropertiesHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext { HttpMethod = "GET", Path = ApiRoutes.ComponentProperties };
            context.SetQueryParameter("instanceId", "123");
            context.SetQueryParameter("componentType", "Transform");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("Transform", context.ResponseBody);
            StringAssert.Contains("m_LocalPosition", context.ResponseBody);
        }

        [Test]
        public void HandleProperties_Returns400_WhenInstanceIdMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyComponentOperations();
            var useCase = new GetComponentPropertiesUseCase(dispatcher, ops);
            var handler = new ComponentPropertiesHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext { HttpMethod = "GET", Path = ApiRoutes.ComponentProperties };

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("instanceId", context.ResponseBody);
        }

        [Test]
        public void HandleProperties_Returns400_WhenComponentTypeMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyComponentOperations();
            var useCase = new GetComponentPropertiesUseCase(dispatcher, ops);
            var handler = new ComponentPropertiesHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext { HttpMethod = "GET", Path = ApiRoutes.ComponentProperties };
            context.SetQueryParameter("instanceId", "123");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("componentType", context.ResponseBody);
        }
    }
}
