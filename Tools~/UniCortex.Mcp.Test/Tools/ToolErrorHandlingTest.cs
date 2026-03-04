using ModelContextProtocol.Protocol;
using NUnit.Framework;
using UniCortex.Mcp.Tools;

namespace UniCortex.Mcp.Test.Tools;

[TestFixture]
public class ToolErrorHandlingTest
{
    [Test]
    public void CreateErrorResult_UsesExceptionMessage()
    {
        var result = ToolErrorHandling.CreateErrorResult(new InvalidOperationException("failed to open scene"));

        Assert.That(result.IsError, Is.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Is.EqualTo("failed to open scene"));
        Assert.That(text, Does.Not.Contain("InvalidOperationException"));
    }

    [Test]
    public void CreateErrorResult_UsesFallbackMessage_WhenExceptionMessageIsEmpty()
    {
        var result = ToolErrorHandling.CreateErrorResult(new Exception(""));

        Assert.That(result.IsError, Is.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Is.EqualTo("Unexpected error."));
    }
}
