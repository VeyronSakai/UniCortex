using NUnit.Framework;
using ModelContextProtocol.Protocol;
using UniCortex.Mcp.Test.Fixtures;

namespace UniCortex.Mcp.Test.ConsoleTools;

[TestFixture]
public class ConsoleLogsTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async Task GetConsoleLogs_ReturnsJsonWithLogs()
    {
        var consoleTools = _fixture.ConsoleTools;

        var result = await consoleTools.GetConsoleLogs(cancellationToken: CancellationToken.None);

        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("logs"));
    }

    [Test]
    public async Task GetConsoleLogs_WithCount_ReturnsLimitedEntries()
    {
        var consoleTools = _fixture.ConsoleTools;

        var result = await consoleTools.GetConsoleLogs(count: 5, cancellationToken: CancellationToken.None);

        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("logs"));
    }

    [Test]
    public async Task ClearConsoleLogs_ReturnsSuccess()
    {
        var consoleTools = _fixture.ConsoleTools;

        var result = await consoleTools.ClearConsoleLogs(CancellationToken.None);

        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("cleared successfully"));
    }
}
