using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SetSceneViewCameraUseCaseTest
    {
        [Test]
        public void ExecuteAsync_SetsSceneViewCamera_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyEditorWindowOperations();
            var useCase = new SetSceneViewCameraUseCase(dispatcher, operations);
            var request = new SetSceneViewCameraRequest
            {
                position = new Vector3Data { x = 1f, y = 2f, z = 3f },
                rotation = new QuaternionData { x = 0f, y = 0f, z = 0f, w = 1f },
                size = 5f,
                orthographic = true
            };

            var result = useCase.ExecuteAsync(request, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(true, result.success);
            Assert.AreEqual(1, operations.SetSceneViewCameraCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
            Assert.NotNull(operations.LastSceneViewCameraRequest);
            Assert.AreEqual(1f, operations.LastSceneViewCameraRequest!.position!.x);
            Assert.AreEqual(1f, operations.LastSceneViewCameraRequest.rotation!.w);
            Assert.AreEqual(5f, operations.LastSceneViewCameraRequest.size);
            Assert.AreEqual(true, operations.LastSceneViewCameraRequest.orthographic);
        }
    }
}
