using NUnit.Framework;
using ModelContextProtocol.Protocol;
using UniCortex.Mcp.Test.Fixtures;

namespace UniCortex.Mcp.Test.PrefabTools;

[TestFixture]
public class PrefabToolsTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async Task InstantiatePrefab_ReturnsError_WhenPrefabNotFound()
    {
        var prefabTools = _fixture.PrefabTools;

        var result = await prefabTools.InstantiatePrefab("Assets/NonExistent.prefab", CancellationToken.None);

        Assert.That(result.IsError, Is.True);
    }
}
