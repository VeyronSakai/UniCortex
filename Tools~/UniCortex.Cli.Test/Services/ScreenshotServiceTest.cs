using NUnit.Framework;
using UniCortex.Cli.Test.Fixtures;

namespace UniCortex.Cli.Test.Services;

[TestFixture]
public class ScreenshotServiceTest
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
        await _fixture.EditorService.EnterPlayModeAsync(CancellationToken.None);
        try
        {
            var pngData = await _fixture.ScreenshotService.CaptureAsync(CancellationToken.None);

            Assert.That(pngData.Length, Is.GreaterThan(0));
        }
        finally
        {
            await _fixture.EditorService.ExitPlayModeAsync(CancellationToken.None);
        }
    }
}
