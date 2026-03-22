using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class GameViewUseCaseTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async ValueTask Capture_InPlayMode_ReturnsPngData()
    {
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        await _fixture.EditorUseCase.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            var pngData = await _fixture.GameViewUseCase.CaptureAsync(CancellationToken.None);

            Assert.That(pngData.Length, Is.GreaterThan(0));
        }
        finally
        {
            await _fixture.EditorUseCase.ExitPlayModeAsync(CancellationToken.None);
        }
    }
}
