using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.ScriptableObject;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class SetScriptableObjectPropertyHandlerTest
    {
        [Test]
        public void HandleSetProperty_Returns200_WhenValid()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyScriptableObjectOperations();
            var useCase = new SetScriptableObjectPropertyUseCase(dispatcher, operations);
            var handler = new SetScriptableObjectPropertyHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.ScriptableObjectSetProperty,
                "{\"assetPath\":\"Assets/Test.asset\",\"propertyPath\":\"m_Name\",\"value\":\"NewName\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual("m_Name", operations.LastSetPropertyPath);
        }

        [Test]
        public void HandleSetProperty_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyScriptableObjectOperations();
            var useCase = new SetScriptableObjectPropertyUseCase(dispatcher, operations);
            var handler = new SetScriptableObjectPropertyHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.ScriptableObjectSetProperty);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("assetPath, propertyPath, and value are required", context.ResponseBody);
        }

        [Test]
        public void HandleSetProperty_Returns400_WhenPropertyPathMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyScriptableObjectOperations();
            var useCase = new SetScriptableObjectPropertyUseCase(dispatcher, operations);
            var handler = new SetScriptableObjectPropertyHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.ScriptableObjectSetProperty,
                "{\"assetPath\":\"Assets/Test.asset\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("propertyPath is required", context.ResponseBody);
        }
    }
}
