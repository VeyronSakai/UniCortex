using System.Text.Json;
using NUnit.Framework;
using ModelContextProtocol.Protocol;
using UniCortex.Editor.Domains.Models;
using UniCortex.Mcp.Test.Fixtures;

namespace UniCortex.Mcp.Test.EditorTools;

[TestFixture]
public class RunTestsTest
{
    private static readonly JsonSerializerOptions JsonOptions = new() { IncludeFields = true };

    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test, CancelAfter(300_000)]
    public async Task RunTests_EditMode_ReturnsJsonWithResults()
    {
        // Arrange
        var editorTools = _fixture.EditorTools;

        // Act
        var result = await editorTools.RunTests(testMode: "EditMode", cancellationToken: CancellationToken.None);

        // Assert
        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var json = ((TextContentBlock)result.Content[0]).Text;
        var response = JsonSerializer.Deserialize<RunTestsResponse>(json, JsonOptions);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.passed + response.failed + response.skipped, Is.GreaterThan(0));
    }

    [Test, CancelAfter(300_000)]
    public async Task RunTests_PlayMode_ReturnsJson()
    {
        // Arrange
        var editorTools = _fixture.EditorTools;

        // Act
        var result = await editorTools.RunTests(testMode: "PlayMode", cancellationToken: CancellationToken.None);

        // Assert
        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var json = ((TextContentBlock)result.Content[0]).Text;
        var response = JsonSerializer.Deserialize<RunTestsResponse>(json, JsonOptions);
        Assert.That(response, Is.Not.Null);
    }

    [Test, CancelAfter(300_000)]
    public async Task RunTests_WithNameFilter_ReturnsFilteredResults()
    {
        // Arrange
        var editorTools = _fixture.EditorTools;

        // Act — filter to a test name that is unlikely to match anything
        var result = await editorTools.RunTests(
            testMode: "EditMode",
            nameFilter: "NonExistentTestName_12345",
            cancellationToken: CancellationToken.None);

        // Assert
        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var json = ((TextContentBlock)result.Content[0]).Text;
        var response = JsonSerializer.Deserialize<RunTestsResponse>(json, JsonOptions);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.results, Has.Count.EqualTo(0));
    }
}
