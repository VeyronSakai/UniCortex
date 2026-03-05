using NUnit.Framework;
using ModelContextProtocol.Protocol;
using UniCortex.Mcp.Test.Fixtures;

namespace UniCortex.Mcp.Test.AssetTools;

[TestFixture]
public class AssetToolsTest
{
    private AssetToolsFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await AssetToolsFixture.CreateAsync();
    }

    [Test]
    public async ValueTask RefreshAssetDatabase_ReturnsSuccess()
    {
        var assetTools = _fixture.AssetTools;

        var result = await assetTools.RefreshAssetDatabase(CancellationToken.None);

        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("refreshed"));
    }
}
