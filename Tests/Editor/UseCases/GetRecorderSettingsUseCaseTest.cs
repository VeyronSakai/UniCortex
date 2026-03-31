using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetRecorderSettingsUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsSettings_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyRecordingOperations
            {
                SettingsResult = new GetRecorderSettingsResponse(
                    "/tmp/out.mp4", "Camera", "MainCamera", "", false, "MP4")
            };
            var useCase = new GetRecorderSettingsUseCase(dispatcher, operations);

            var result = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("/tmp/out.mp4", result.outputPath);
            Assert.AreEqual("Camera", result.source);
            Assert.AreEqual("MP4", result.outputFormat);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
