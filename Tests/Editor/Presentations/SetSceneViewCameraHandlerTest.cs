using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.SceneView;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;
using UnityEngine;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class SetSceneViewCameraHandlerTest
    {
        [Test]
        public void HandleSetSceneViewCamera_Returns200WithSuccess()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyEditorWindowOperations();
            var useCase = new SetSceneViewCameraUseCase(dispatcher, operations);
            var handler = new SetSceneViewCameraHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var request = new SetSceneViewCameraRequest
            {
                position = new Vector3Data { x = 0f, y = 1f, z = -10f },
                rotation = new QuaternionData { x = 0f, y = 0f, z = 0f, w = 1f },
                size = 7.5f,
                orthographic = false
            };
            var body = JsonUtility.ToJson(request);
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.SceneViewCamera, body);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(1, operations.SetSceneViewCameraCallCount);
            Assert.NotNull(operations.LastSceneViewCameraRequest);
            Assert.AreEqual(-10f, operations.LastSceneViewCameraRequest!.position!.z);
            Assert.AreEqual(7.5f, operations.LastSceneViewCameraRequest.size);
            Assert.AreEqual(false, operations.LastSceneViewCameraRequest.orthographic);
            StringAssert.Contains("true", context.ResponseBody);
        }
    }
}
