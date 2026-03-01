using NUnit.Framework;
using ModelContextProtocol.Protocol;
using UniCortex.Mcp.Test.Fixtures;

namespace UniCortex.Mcp.Test.ScriptableObjectTools;

[TestFixture]
public class ScriptableObjectToolsTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async Task CreateScriptableObject_ReturnsSuccess()
    {
        var tools = _fixture.ScriptableObjectTools;
        const string AssetPath = "Assets/CreateScriptableObjectTest.asset";

        try
        {
            var result = await tools.CreateScriptableObject("AllPropertyTypesScriptableObject", AssetPath, CancellationToken.None);

            Assert.That(result.IsError, Is.Not.True);
            Assert.That(result.Content, Has.Count.EqualTo(1));
            var text = ((TextContentBlock)result.Content[0]).Text;
            Assert.That(text, Does.Contain($"ScriptableObject created at: {AssetPath}"));
        }
        finally
        {
            UnityEditorFixture.DeleteAssetFile(AssetPath);
        }
    }

    [Test]
    public async Task SetScriptableObjectProperty_ReturnsSuccess()
    {
        var tools = _fixture.ScriptableObjectTools;
        const string AssetPath = "Assets/SetScriptableObjectPropertyTest.asset";

        try
        {
            await tools.CreateScriptableObject("AllPropertyTypesScriptableObject", AssetPath, CancellationToken.None);

            var result = await tools.SetScriptableObjectProperty(AssetPath, "m_Name", "RenamedAsset",
                CancellationToken.None);

            Assert.That(result.IsError, Is.Not.True);
            Assert.That(result.Content, Has.Count.EqualTo(1));
            var text = ((TextContentBlock)result.Content[0]).Text;
            Assert.That(text, Does.Contain($"Property 'm_Name' set on ScriptableObject '{AssetPath}'."));
        }
        finally
        {
            UnityEditorFixture.DeleteAssetFile(AssetPath);
        }
    }
}
