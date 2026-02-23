using ModelContextProtocol.Protocol;
using UniCortex.Mcp.IntegrationTests;

namespace UniCortex.Mcp.IntegrationTests.Tools;

[TestFixture]
[Category("Integration")]
public class PauseResumeTest
{
    [SetUp]
    public async Task SetUp()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        await UnityEditorFixture.EditorTools.EnterPlayMode(cts.Token);
    }

    [TearDown]
    public async Task TearDown()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
        await UnityEditorFixture.EditorTools.ExitPlayMode(cts.Token);
    }

    [Test]
    public async Task PauseEditor_Succeeds()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));

        var result = await UnityEditorFixture.EditorTools.PauseEditor(cts.Token);

        Assert.That(result.IsError, Is.Not.True);
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("Paused successfully"));
    }

    [Test]
    public async Task ResumeEditor_Succeeds()
    {
        using var pauseCts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
        await UnityEditorFixture.EditorTools.PauseEditor(pauseCts.Token);

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));

        var result = await UnityEditorFixture.EditorTools.ResumeEditor(cts.Token);

        Assert.That(result.IsError, Is.Not.True);
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("Resumed successfully"));
    }
}
