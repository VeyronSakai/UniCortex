using System.Collections.Generic;
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
    internal sealed class ScriptableObjectInfoHandlerTest
    {
        [Test]
        public void HandleInfo_Returns200_WithScriptableObjectInfo()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyScriptableObjectOperations
            {
                GetInfoResult = new ScriptableObjectInfoResponse("Assets/Test.asset", "TestConfig",
                    new List<SerializedPropertyEntry>
                    {
                        new SerializedPropertyEntry("m_Name", "String", "Test")
                    })
            };
            var useCase = new GetScriptableObjectInfoUseCase(dispatcher, operations);
            var handler = new ScriptableObjectInfoHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("GET", ApiRoutes.ScriptableObjectInfo);
            context.SetQueryParameter("assetPath", "Assets/Test.asset");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("TestConfig", context.ResponseBody);
            StringAssert.Contains("m_Name", context.ResponseBody);
        }

        [Test]
        public void HandleInfo_Returns400_WhenAssetPathMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyScriptableObjectOperations();
            var useCase = new GetScriptableObjectInfoUseCase(dispatcher, operations);
            var handler = new ScriptableObjectInfoHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("GET", ApiRoutes.ScriptableObjectInfo);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("assetPath query parameter is required", context.ResponseBody);
        }
    }
}
