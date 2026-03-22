using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class ScreenshotUseCaseTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async ValueTask CaptureGameView_InPlayMode_ReturnsPngData()
    {
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            var pngData = await _fixture.ScreenshotUseCase.CaptureGameViewAsync(CancellationToken.None);

            Assert.That(pngData.Length, Is.GreaterThan(0));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }

    [Test]
    public async ValueTask CaptureSceneView_ReturnsPngData()
    {
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);

        var pngData = await _fixture.ScreenshotUseCase.CaptureSceneViewAsync(CancellationToken.None);

        Assert.That(pngData.Length, Is.GreaterThan(0));
    }

}
