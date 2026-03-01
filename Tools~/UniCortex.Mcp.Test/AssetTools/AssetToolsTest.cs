using NUnit.Framework;
using ModelContextProtocol.Protocol;
using UniCortex.Mcp.Test.Fixtures;

namespace UniCortex.Mcp.Test.AssetTools;

[TestFixture]
public class AssetToolsTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async Task RefreshAssetDatabase_ReturnsSuccess()
    {
        var assetTools = _fixture.AssetTools;

        var result = await assetTools.RefreshAssetDatabase(CancellationToken.None);

        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("refreshed"));
    }

    [Test]
    public async Task CreateAsset_ReturnsSuccess()
    {
        var assetTools = _fixture.AssetTools;
        const string AssetPath = "Assets/CreateAssetTest.asset";

        try
        {
            var result = await assetTools.CreateAsset("AllPropertyTypesScriptableObject", AssetPath, CancellationToken.None);

            Assert.That(result.IsError, Is.Not.True);
            Assert.That(result.Content, Has.Count.EqualTo(1));
            var text = ((TextContentBlock)result.Content[0]).Text;
            Assert.That(text, Does.Contain($"Asset created at: {AssetPath}"));
        }
        finally
        {
            UnityEditorFixture.DeleteAssetFile(AssetPath);
        }
    }

    [Test]
    public async Task SetAssetProperty_ReturnsSuccess()
    {
        var assetTools = _fixture.AssetTools;
        const string AssetPath = "Assets/SetAssetPropertyTest.asset";

        try
        {
            await assetTools.CreateAsset("AllPropertyTypesScriptableObject", AssetPath, CancellationToken.None);

            var result = await assetTools.SetAssetProperty(AssetPath, "m_Name", "RenamedAsset",
                CancellationToken.None);

            Assert.That(result.IsError, Is.Not.True);
            Assert.That(result.Content, Has.Count.EqualTo(1));
            var text = ((TextContentBlock)result.Content[0]).Text;
            Assert.That(text, Does.Contain($"Property 'm_Name' set on asset '{AssetPath}'."));
        }
        finally
        {
            UnityEditorFixture.DeleteAssetFile(AssetPath);
        }
    }
}
