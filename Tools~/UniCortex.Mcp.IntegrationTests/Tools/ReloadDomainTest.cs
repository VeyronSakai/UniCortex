using ModelContextProtocol.Protocol;
using UniCortex.Mcp.IntegrationTests;

namespace UniCortex.Mcp.IntegrationTests.Tools;

[TestFixture]
[Category("Integration")]
public class ReloadDomainTest
{
    [Test]
    public async Task ReloadDomain_Succeeds()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

        var result = await UnityEditorFixture.EditorTools.ReloadDomain(cts.Token);

        Assert.That(result.IsError, Is.Not.True);
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("Domain reload completed"));
    }
}
