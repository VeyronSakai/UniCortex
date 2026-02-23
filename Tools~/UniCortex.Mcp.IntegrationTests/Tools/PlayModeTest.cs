using ModelContextProtocol.Protocol;
using UniCortex.Mcp.IntegrationTests;

namespace UniCortex.Mcp.IntegrationTests.Tools;

[TestFixture]
[Category("Integration")]
public class PlayModeTest
{
    [TearDown]
    public async Task TearDown()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
        await UnityEditorFixture.EditorTools.ExitPlayMode(cts.Token);
    }

    [Test]
    public async Task EnterPlayMode_Succeeds()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

        var result = await UnityEditorFixture.EditorTools.EnterPlayMode(cts.Token);

        Assert.That(result.IsError, Is.Not.True);
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("Play mode started"));
    }

    [Test]
    public async Task ExitPlayMode_Succeeds()
    {
        using var enterCts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        await UnityEditorFixture.EditorTools.EnterPlayMode(enterCts.Token);

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));

        var result = await UnityEditorFixture.EditorTools.ExitPlayMode(cts.Token);

        Assert.That(result.IsError, Is.Not.True);
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("Play mode stopped"));
    }
}
