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
    internal sealed class CreateScriptableObjectHandlerTest
    {
        [Test]
        public void HandleCreate_Returns200_WhenValid()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyScriptableObjectOperations();
            var useCase = new CreateScriptableObjectUseCase(dispatcher, operations);
            var handler = new CreateScriptableObjectHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.ScriptableObjectCreate,
                "{\"type\":\"TestConfig\",\"assetPath\":\"Assets/TestConfig.asset\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual("TestConfig", operations.LastCreateType);
        }

        [Test]
        public void HandleCreate_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyScriptableObjectOperations();
            var useCase = new CreateScriptableObjectUseCase(dispatcher, operations);
            var handler = new CreateScriptableObjectHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.ScriptableObjectCreate);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("type and assetPath are required", context.ResponseBody);
        }

        [Test]
        public void HandleCreate_Returns400_WhenTypeMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyScriptableObjectOperations();
            var useCase = new CreateScriptableObjectUseCase(dispatcher, operations);
            var handler = new CreateScriptableObjectHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.ScriptableObjectCreate,
                "{\"assetPath\":\"Assets/TestConfig.asset\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("type is required", context.ResponseBody);
        }
    }
}
