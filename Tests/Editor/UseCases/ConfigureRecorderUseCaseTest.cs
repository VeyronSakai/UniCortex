using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class ConfigureRecorderUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsConfigureRecorder_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyRecordingOperations();
            var useCase = new ConfigureRecorderUseCase(dispatcher, operations);

            useCase.ExecuteAsync("/tmp/out.mp4", "Camera", "MainCamera", "Player",
                true, 1920, 1080, "MP4", CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.ConfigureCallCount);
            Assert.AreEqual("/tmp/out.mp4", operations.LastConfigOutputPath);
            Assert.AreEqual("Camera", operations.LastConfigSource);
            Assert.AreEqual("MainCamera", operations.LastConfigCameraSource);
            Assert.AreEqual("Player", operations.LastConfigCameraTag);
            Assert.IsTrue(operations.LastConfigCaptureUI);
            Assert.AreEqual(1920, operations.LastConfigOutputWidth);
            Assert.AreEqual(1080, operations.LastConfigOutputHeight);
            Assert.AreEqual("MP4", operations.LastConfigOutputFormat);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
