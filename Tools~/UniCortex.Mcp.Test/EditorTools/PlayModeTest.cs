using NUnit.Framework;
using ModelContextProtocol.Protocol;
using UniCortex.Mcp.Test.Fixtures;

namespace UniCortex.Mcp.Test.EditorTools;

[TestFixture]
[FixtureLifeCycle(LifeCycle.SingleInstance)]
public class PlayModeTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();

        // Ensure not in play mode before tests (idempotent: no-op if already stopped)
        await _fixture.EditorTools.ExitPlayModeAsync(CancellationToken.None);
    }

    [Test, Order(1)]
    public async ValueTask EnterPlayMode_ReturnsSuccess()
    {
        var result = await _fixture.EditorTools.EnterPlayModeAsync(CancellationToken.None);

        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("started"));
    }

    [Test, Order(2)]
    public async ValueTask ExitPlayMode_ReturnsSuccess()
    {
        var result = await _fixture.EditorTools.ExitPlayModeAsync(CancellationToken.None);

        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("stopped"));
    }

    [OneTimeTearDown]
    public async ValueTask OneTimeTearDown()
    {
        // Safety: ensure play mode is stopped (idempotent: no-op if already stopped)
        await _fixture.EditorTools.ExitPlayModeAsync(CancellationToken.None);
    }
}
