using NUnit.Framework;
using ModelContextProtocol.Protocol;
using UniCortex.Mcp.Test.Fixtures;

namespace UniCortex.Mcp.Test.UtilityTools;

[TestFixture]
public class UtilityToolsTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async Task ExecuteMenuItem_ReturnsSuccess()
    {
        var utilityTools = _fixture.UtilityTools;

        var result = await utilityTools.ExecuteMenuItem("Edit/Select All", CancellationToken.None);

        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("executed"));
    }

    [Test]
    public async Task CaptureScreenshot_ReturnsImage()
    {
        var utilityTools = _fixture.UtilityTools;

        var result = await utilityTools.CaptureScreenshot(CancellationToken.None);

        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var image = (ImageContentBlock)result.Content[0];
        Assert.That(image.MimeType, Is.EqualTo("image/png"));
        Assert.That(image.Data.Length, Is.GreaterThan(0));
    }
}
