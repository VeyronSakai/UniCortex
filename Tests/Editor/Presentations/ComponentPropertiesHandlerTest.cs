using System.Collections.Generic;
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
    internal sealed class ComponentPropertiesHandlerTest
    {
        [Test]
        public void HandleProperties_Returns200_WithProperties()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyComponentOperations();
            ops.GetPropertiesResult = new GetComponentPropertiesResponse("UnityEngine.Transform",
                new List<SerializedPropertyEntry>
                {
                    new SerializedPropertyEntry("m_LocalPosition", "Vector3", "(0, 0, 0)")
                });
            var useCase = new GetComponentPropertiesUseCase(dispatcher, ops);
            var handler = new ComponentPropertiesHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.ComponentProperties);
            context.SetQueryParameter("instanceId", "123");
            context.SetQueryParameter("componentType", "UnityEngine.Transform");
            context.SetQueryParameter("assemblyName", "UnityEngine.CoreModule");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("UnityEngine.Transform", context.ResponseBody);
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

            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.ComponentProperties);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
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

            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.ComponentProperties);
            context.SetQueryParameter("instanceId", "123");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("componentType", context.ResponseBody);
        }

        private static (RequestRouter router, FakeRequestContext context, SpyComponentOperations ops)
            CreatePropertiesRequest()
        {
            var ops = new SpyComponentOperations
            {
                GetPropertiesResult = new GetComponentPropertiesResponse("UnityEngine.Playables.PlayableDirector",
                    new List<SerializedPropertyEntry>())
            };
            var router = new RequestRouter();
            new ComponentPropertiesHandler(new GetComponentPropertiesUseCase(new FakeMainThreadDispatcher(), ops))
                .Register(router);
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.ComponentProperties);
            context.SetQueryParameter("instanceId", "123");
            context.SetQueryParameter("componentType", "UnityEngine.Playables.PlayableDirector");
            context.SetQueryParameter("assemblyName", "UnityEngine.DirectorModule");
            return (router, context, ops);
        }

        [Test]
        public void HandleProperties_WithoutPropertyPathAndDepth_ListsEveryLevel()
        {
            // Arrange
            var (router, context, ops) = CreatePropertiesRequest();

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.IsNull(ops.LastGetPropertiesPropertyPath);
            Assert.IsNull(ops.LastGetPropertiesDepth);
        }

        [Test]
        public void HandleProperties_PassesPropertyPathAndDepth()
        {
            // Arrange
            var (router, context, ops) = CreatePropertiesRequest();
            context.SetQueryParameter("propertyPath", "m_SceneBindings.Array.data[0]");
            context.SetQueryParameter("depth", "1");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual("m_SceneBindings.Array.data[0]", ops.LastGetPropertiesPropertyPath);
            Assert.AreEqual(1, ops.LastGetPropertiesDepth);
        }

        [TestCase("abc", "depth must be an integer")]
        [TestCase("-1", "depth must be 0 or greater")]
        public void HandleProperties_WithInvalidDepth_Returns400(string depth, string expectedMessage)
        {
            // Arrange
            var (router, context, ops) = CreatePropertiesRequest();
            context.SetQueryParameter("depth", depth);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains(expectedMessage, context.ResponseBody);
            Assert.AreEqual(0, ops.GetPropertiesCallCount);
        }
    }
}
