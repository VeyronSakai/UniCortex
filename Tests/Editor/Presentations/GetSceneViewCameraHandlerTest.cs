using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.SceneView;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class GetSceneViewCameraHandlerTest
    {
        [Test]
        public void HandleGetSceneViewCamera_Returns200WithCameraState()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyEditorWindowOperations
            {
                SceneViewCameraResponse = new GetSceneViewCameraResponse(
                    new Vector3Data { x = 10f, y = 20f, z = 30f },
                    new QuaternionData { x = 0f, y = 0f, z = 0f, w = 1f },
                    6f,
                    false)
            };
            var useCase = new GetSceneViewCameraUseCase(dispatcher, operations);
            var handler = new GetSceneViewCameraHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.SceneViewCamera);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("10.0", context.ResponseBody);
            StringAssert.Contains("30.0", context.ResponseBody);
            StringAssert.Contains("6.0", context.ResponseBody);
            StringAssert.Contains("false", context.ResponseBody);
            Assert.AreEqual(1, operations.GetSceneViewCameraCallCount);
        }
    }
}
