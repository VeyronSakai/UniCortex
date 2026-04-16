using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetSceneViewCameraUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsSceneViewCamera_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyEditorWindowOperations
            {
                SceneViewCameraResponse = new GetSceneViewCameraResponse(
                    new Vector3Data { x = 1f, y = 2f, z = 3f },
                    new QuaternionData { x = 0f, y = 0.5f, z = 0f, w = 0.5f },
                    4f,
                    true)
            };
            var useCase = new GetSceneViewCameraUseCase(dispatcher, operations);

            var result = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1f, result.position.x);
            Assert.AreEqual(0.5f, result.rotation.y);
            Assert.AreEqual(4f, result.size);
            Assert.AreEqual(true, result.orthographic);
            Assert.AreEqual(1, operations.GetSceneViewCameraCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
