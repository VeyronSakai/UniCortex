using NUnit.Framework;
using ModelContextProtocol.Protocol;
using UniCortex.Mcp.Test.Fixtures;

namespace UniCortex.Mcp.Test.ScreenshotTools;

[TestFixture]
public class ScreenshotToolsTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async Task CaptureScreenshot_InPlayMode_ReturnsImage()
    {
        var editorTools = _fixture.EditorTools;
        var screenshotTools = _fixture.ScreenshotTools;

        await editorTools.EnterPlayMode(CancellationToken.None);
        try
        {
            var result = await screenshotTools.CaptureScreenshot(CancellationToken.None);

            Assert.That(result.IsError, Is.Not.True);
            Assert.That(result.Content, Has.Count.EqualTo(1));
            var image = (ImageContentBlock)result.Content[0];
            Assert.That(image.MimeType, Is.EqualTo("image/png"));
            Assert.That(image.Data.Length, Is.GreaterThan(0));
        }
        finally
        {
            await editorTools.ExitPlayMode(CancellationToken.None);
        }
    }
}
