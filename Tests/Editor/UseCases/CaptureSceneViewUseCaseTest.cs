using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class CaptureSceneViewUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsData_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyCaptureOperations
            {
                ScreenshotResult = new byte[] { 0x89, 0x50, 0x4E, 0x47 }
            };
            var useCase = new CaptureSceneViewUseCase(dispatcher, operations);

            var result = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.CaptureSceneViewCallCount);
            Assert.AreEqual(4, result.Length);
            Assert.AreEqual(0x89, result[0]);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
