using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class RecordingUseCaseTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async ValueTask ConfigureAndGetSettings_ReturnsConfiguredValues()
    {
        await _fixture.RecordingUseCase.ConfigureAsync(
            "/tmp/test.mp4", "GameView", null, null,
            false, 1920, 1080, "MP4", CancellationToken.None);

        var settings = await _fixture.RecordingUseCase.GetSettingsAsync(CancellationToken.None);
        Assert.That(settings.outputPath, Is.EqualTo("/tmp/test.mp4"));
        Assert.That(settings.source, Is.EqualTo("GameView"));
        Assert.That(settings.outputWidth, Is.EqualTo(1920));
        Assert.That(settings.outputHeight, Is.EqualTo(1080));
        Assert.That(settings.outputFormat, Is.EqualTo("MP4"));
    }

    [Test]
    public async ValueTask StartAndStop_InPlayMode_ReturnsOutputPath()
    {
        await _fixture.RecordingUseCase.ConfigureAsync(
            null, "GameView", null, null,
            false, 0, 0, "MP4", CancellationToken.None);

        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            var startMessage = await _fixture.RecordingUseCase.StartAsync(
                30, "Constant", "Manual",
                0, 0, 0, 0, 0, CancellationToken.None);
            Assert.That(startMessage, Is.Not.Empty);

            await Task.Delay(1000);

            var outputPath = await _fixture.RecordingUseCase.StopAsync(CancellationToken.None);
            Assert.That(outputPath, Is.Not.Empty);
            Assert.That(File.Exists(outputPath), Is.True, $"Recording file should exist at: {outputPath}");
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }
}
