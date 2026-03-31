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
    public async ValueTask StartAndStop_InPlayMode_ReturnsOutputPath()
    {
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            var startMessage = await _fixture.RecordingUseCase.StartAsync(30, null, CancellationToken.None);
            Assert.That(startMessage, Is.Not.Empty);

            // Record a short duration
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
